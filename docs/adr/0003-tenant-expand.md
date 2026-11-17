# ADR 0003: Tenant Migration — Expand Phase

## Status:
Accepted

## Date:
2026-11-16

## Context

[ADR 0002](./0002-tenancy-model.md) chose the pool model (shared database, shared schema, `TenantId` discriminator) and introduced `Tenant` and `TenantMembership` in `AuthDbContext`. The inventory tables (`SmartInventoryDbContext`) still have no tenant column, and they may already hold production-like data that belongs to no tenant.

Adding `TenantId` as `NOT NULL` with a foreign key in one step would fail on existing rows. To emulate a real migration of a database with production data, the change follows the **Expand → Backfill → Contract** pattern, with each phase deployable and reversible on its own:

| Phase | Goal | State of the database afterwards |
|---|---|---|
| **Expand** (this ADR) | Add the new structure without changing behaviour. | `TenantId` exists, is `NULL`able, unconstrained and unused by queries. |
| **Backfill** | Assign every existing row to a tenant. | No row has `TenantId IS NULL`. |
| **Contract** | Enforce the new invariant. | `TenantId` is `NOT NULL` with a foreign key. |

This ADR covers only the Expand phase. The code for it is already implemented.

## Decision

### 1. Add a nullable `TenantId` to the inventory entities

- A new marker interface, `ITenantEntity` (`SmartInventory.Domain/Entities`), exposes `Guid? TenantId { get; set; }`.
- It is implemented by all eight tenant-scoped entities: `Product`, `Warehouse`, `Stock`, `StockTransaction`, `Supplier`, `PurchaseOrder`, `PurchaseOrderItem` and `StockAlert`.
- Each entity configuration declares the property explicitly with `IsRequired(false)`.
- Following ADR 0002 §2.4, `TenantId` is a **non-navigation soft FK**. There is no navigation property to `Tenant`, because `Tenant` lives in the other DbContext.
- The property has a public setter so that seeders and the later backfill can stamp it. The `Guid?` type is temporary and becomes `Guid` in the Contract phase.

### 2. EF Core migration `20261116160331_tenant-expand` (Inventory context)

- It only adds eight `uniqueidentifier NULL` columns, one for each entity above, in the `Inventory`, `Purchasing` and `Alerts` schemas.
- `Down()` drops the same columns, which makes rollback safe and cheap.
- It adds **no** index, foreign key, default value or unique-constraint change. These belong to the Contract phase and to the per-tenant uniqueness work described in ADR 0002 §2.5.

### 3. Legacy tenant data for the later Backfill

- `SystemTenants.LegacyTenantId` (`99999999-0000-0000-0000-000000000100`) is a well-known constant for the tenant that will own all pre-existing rows.
- `TenantMigrationSeeder.MigrateLegacyTenant(filePath)` reads `Data/legacy_tenant.json` and inserts the `Tenant`, the `TenantMembership` and the admin `User` in `AuthDbContext`. The user's password is hashed at insert time.
- The seeder validates the file. It requires `tenant`, `membership` and `user` objects, non-empty IDs, `membership.TenantId == tenant.Id` and `membership.UserId == user.Id`. Otherwise it throws `InvalidOperationException`.
- It is run on demand with the seeder console command `tenant-migration-expand`. It is deliberately **not** part of the default "seed all" run.
- The seeder is idempotent. A second run will noe fail on the duplicate primary key. 

### 4. Clean-deployment seed data

- New `TenantSeeder` and `TenantMembershipSeeder` load `tenants.json` and `tenant_memberships.json`. They are registered in `DatabaseSeeder` and in the console command list.
- The seed JSON files for the inventory entities (`products`, `warehouses`, `stocks`, `stock_transactions`, `suppliers`, `purchase_orders`, `purchase_order_items`) now include `TenantId`.
- These seeders are for **clean deployments only**. Existing databases follow the legacy-tenant path (section 3) plus the Backfill phase.
- Supporting seeder changes:
  - Seeders now skip a missing data file instead of throwing.
  - `UserSeeder` inserts only if there is not users data. The previous update-if-exists branch was removed.
  - `SeederSettings` became public so that it can be used from the integration tests.

### 5. Tenant context stub

`TenantContext` (API) no longer throws `NotImplementedException`. It returns `IsAvailable = false` and `IsTenantScoped = false`, and `TenantId` throws `InvalidOperationException`. This matches ADR 0002's "single-tenant behaviour until M4" rule and keeps the API usable during the Expand phase.

### 6. Test

`TenantMigrationTests.MigrateLegacyTenant_PersistsLegacyValues` uses the shared Testcontainers SQL Server fixture from ADR 0001. It runs the seeder against `legacy_tenant.json` and asserts the persisted `Tenant`, `TenantMembership` and `User` values.

### Out of scope for this phase

- Populating `TenantId` on existing rows (Backfill).
- `NOT NULL` constraints, foreign keys, indexes on `TenantId` and tenant-scoped unique indexes (Contract; ADR 0002 §2.5).
- Global query filters, the `SaveChangesAsync` interceptor and tenant resolution. ADR 0002 plans these separately, and they have not been started.

## Alternatives

| Option | Why not |
|---|---|
| **Clean deployment only** (drop and recreate the database) | Simplest, but it skips the chance to practise a migration pattern that is valuable on real production data. |
| **One-step migration with SQL scripts** | Faster, but it moves schema changes outside EF Core migrations, which are the source of truth in this code-first project. It also gives no intermediate state to verify or roll back. |
| **One EF migration that adds `NOT NULL` with a default (the legacy tenant ID)** | Short, but it couples schema and data changes. It locks a large table during the rewrite, and it hides the backfill decision, which is still open. |
| **Foreign key to `Tenant` in the Expand phase** | Not possible while the column holds only `NULL`s, and it would add the FK constraint before the backfill is verified. It is deferred to Contract. |

## Consequences

### Benefits

- The Expand change is purely additive. Existing queries, handlers and tests keep working, because nothing reads `TenantId` yet.
- Each phase can be deployed and rolled back independently. For this phase, rollback is `Down()` dropping nullable columns.
- Legacy data has a deterministic owner (`SystemTenants.LegacyTenantId`) before the backfill is written.
- Clean deployments already get realistic multi-tenant seed data.

### Trade-offs

- Three phases mean three merges to `main`. Until Contract ships, the schema has no integrity guarantee, so rows can exist with a `NULL` or an arbitrary `TenantId`.
- There are two seeding paths, clean seed and legacy migration, which must be kept in sync.
- The `Guid?` property and the public `TenantId` setter are temporary weaknesses in the domain model.
- `UserSeeder` is no longer an upsert, and will follow the clean update pattern of the other seeders.

### Risks

- **Rows left without a tenant.** Once code starts filtering by tenant, rows with `NULL` become invisible. Mitigation: Contract must not run until a check confirms zero `NULL` rows in every table.
- **Phase ordering.** Deploying Contract before Backfill fails on the `NOT NULL` constraint. Mitigation: gate the Contract migration on that check.

- **The legacy file contains a plaintext seed password.** It is acceptable for local and demo data, but must not be used for real environments.
- **Rollback.** If the phase fails, run `Down()` to drop the columns. Data written to `TenantId` since deployment is lost.

## Next phases

- **Backfill.** Assign `SystemTenants.LegacyTenantId` to all existing rows in the eight tables. The mechanism (seeder console command or SQL inside an EF migration) is not decided yet. It will be recorded in a follow-up ADR or an update to this one.
- **Contract.** Make `TenantId` `NOT NULL` (`Guid`) and add the foreign key to `Tenant`, the indexes and the per-tenant unique indexes.

## Related ADRs

- [0001-testing-and-engineering-readiness.md](./0001-testing-and-engineering-readiness.md): the Testcontainers fixture used by the integration test.
- [0002-tenancy-model.md](./0002-tenancy-model.md): the pool isolation model, the `Tenant` and `TenantMembership` entities, and the per-tenant uniqueness plan. This ADR implements the data-structure part of its M3 milestone. ADR 0002 reserved the numbers 0003 and 0004 for tenant resolution and domain migration, so its roadmap table and the "ADR 0003 (planned)" references need updating to match.

## Implementation Notes
the TenantMigrationSeeder and the legacy_tenant.json were removed to simplify the migration process