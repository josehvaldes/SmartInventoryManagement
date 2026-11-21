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


----------------------------------------------------------------------------------------------------------------------------------------------------------------

# ADR 0003: Tenant Migration — Backfill Phase

## Status:
Accepted

## Date:
2026-11-16

## Context

The [Expand phase](./0003-tenant-expand.md) added a nullable `TenantId` to the eight tenant-scoped inventory tables. Existing rows still have `TenantId IS NULL`, and the Auth database has no tenant that could own them. The Expand phase also introduced a console command, `tenant-migration-expand`, that created the legacy tenant from `legacy_tenant.json` as a manual step, and left the backfill mechanism open.

This phase must:

- Create the **legacy tenant**, its admin user and its membership.
- Assign every existing row to the legacy tenant.
- Do both without a manual step, so any environment (local, CI, production) reaches the same state just by applying EF Core migrations.

Backfill and Contract ship in the same branch and merge, as separate commits and separate ADRs. Contract is described in its own ADR. Backfill must still leave the database valid on its own: no schema constraint changes, and no `NULL` left in any `TenantId` column.

## Decision

### 1. Backfill runs as EF Core migrations

Two new migrations carry the backfill, one for each DbContext, so each follows that context's normal `MigrateAsync()` path:

| Context | Migration | What it does |
|---|---|---|
| `AuthDbContext` | `20261116203309_TenantBackfill` | Inserts the legacy `Tenant` (`legacy-tenant`, `Active`), the legacy `User` (`legacyuser`) and an `Admin` `TenantMembership` linking them. |
| `SmartInventoryDbContext` | `20261116203249_TenantBackfill` | Runs `UPDATE ... SET TenantId = <legacy tenant id> WHERE TenantId IS NULL` on `Products`, `Warehouses`, `Stock`, `StockTransactions`, `Suppliers`, `PurchaseOrders`, `PurchaseOrderItems` and `StockAlerts`. |

- The `WHERE TenantId IS NULL` filter makes the Inventory update safe to re-run, and it never overwrites rows that already have a tenant.
- The legacy identifiers live in `SystemTenants` (`LegacyTenantId`, `LegacyTenantMembershipId`, `LegacyUserId`). Migrations, the integration tests and the later Contract phase use these constants.
- The inserted columns are `Id`, `Name`, `Slug` and `Status` (`1` = `Active`) for the tenant. The user gets the existing password hash from `legacy_tenant.json`. The membership gets role `1` (`Admin`). `IsActive` and `JoinedAt` rely on their column defaults, and `CreatedAt` is set to `DateTime.UtcNow` at migration time.
- The Inventory data update uses raw SQL, because EF Core has no set-based update in the migration API. The only interpolated value is a `Guid`, so there is no injection risk.

### 2. Retire the migration seeder and its console command

- `TenantMigrationSeeder` and the `tenant-migration-expand` command are removed from the Seeds console app.
- The Seeds app goes back to a single role: seeding **clean deployments**, idempotently. It no longer takes part in migrating existing data.
- `legacy_tenant.json` stays only as a reference for the values the migration inserts. It is no longer read by any code.

### 3. Rework the migration tests

`TenantMigrationTests` no longer shares the integration fixture. That fixture deletes the data between tests so every test starts clean, and that would erase the rows the migrations insert.

- Each test now starts its own SQL Server Testcontainer, applies the migrations with `MigrateAsync()` and checks the result.
- `..._AuthContext` asserts the legacy tenant, membership and user exist with the expected `SystemTenants` identifiers, status and role.
- `..._InventoryContext` asserts that no row in the eight tables has a `TenantId` other than the legacy tenant.

### 4. Supporting changes

- `SmartInventoryDbContext` now exposes `DbSet<StockAlert>`, which the test needs to query alerts.
- The Expand migration's class name and migration ID changed in its `.Designer.cs` file (`tenantexpand` → `Tenantexpand`). See Risks.

### Out of scope for this phase

- `NOT NULL`, foreign keys and indexes on `TenantId`, and tenant-scoped unique indexes. These belong to the Contract phase.
- Stamping `TenantId` on new writes, and tenant resolution. Until then, new rows are still inserted with `NULL`.

## Alternatives

| Option | Why not |
|---|---|
| **Keep the Seeds console tool as the backfill runner** (the Expand design) | It adds a deployment step outside the migration pipeline that someone must remember to run, and it would be a third tool next to the API and the migrations. Environments could end up in different states. |
| **One SQL script run outside EF Core** | The same extra-step problem, and it moves schema and data history out of the code-first migration history. |
| **Backfill lazily in application code on first access** | It leaves `NULL` rows in the database for an unknown time, so Contract could never be applied safely. |
| **Single combined migration for Auth and Inventory** | Not possible, since the two DbContexts have separate migration histories. |

## Consequences

### Benefits

- One mechanism for every environment. Applying migrations is enough, and there is no operator step.
- The Seeds tool is simpler and has a single purpose.
- The backfill is covered by a test that runs the real migrations against a real SQL Server.
- Legacy identifiers are defined once (`SystemTenants`) and shared between migrations and tests.

### Trade-offs

- Data changes now live in schema migrations. They are harder to re-run selectively and they run on every environment, including fresh ones.
- Every new, empty database also receives the legacy tenant and user, even where no legacy data exists.
- Each test in `TenantMigrationTests` starts its own container, so the tests are slower than those using the shared fixture.
- The two DbContexts migrate independently, so there is no guarantee about which one runs first (see Risks).

### Risks


- **Hard-coded credentials.** The legacy user is an `Admin` with a known seed password (the hash is in source control). This is acceptable for local and demo data. Before any real environment, remove the user, or disable it (`IsActive = false`), or replace its password.
- **Migrations depend on mutable constants.** They read `SystemTenants` from the Domain project. If those values change later, an old migration would behave differently on a fresh database. Treat the constants as frozen; copying the literals into the migrations would remove the coupling.
- **Non-deterministic timestamps.** `CreatedAt` and `JoinedAt` use `DateTime.UtcNow` when the migration runs, so the same migration gives different values in different environments.
- **Migration order across contexts.** The API currently migrates Inventory first and then Auth. Backfill is safe in either order because there is no foreign key yet. The Contract phase adds a foreign key to `Tenants`, so Auth must be migrated first there.
- **Rows inserted after the backfill.** The migration only updates rows that exist when it runs. Writes made with `TenantId = NULL` afterwards must be handled before Contract, either by stamping `TenantId` on write or by re-running the update.
- **Changing an applied migration.** The Expand migration ID was changed after it was merged (see Supporting changes). A database that already applied the old ID would try to apply the renamed one again and fail because the columns exist. Keep the original ID.
- **Test coverage.** On an empty database the inventory test passes without any row to update. It proves the migration runs, not that it backfills.

## Next phase

- **Contract** ([ADR 0003: Tenant Migration — Contract Phase](./0003-tenant-contract.md), planned). Make `TenantId` `NOT NULL`, add the foreign key to `Tenants`, and add the indexes and per-tenant unique indexes from ADR 0002 §2.5.

## Related ADRs

- [0002-tenancy-model.md](./0002-tenancy-model.md): the pool isolation model and the legacy-data plan this phase implements.
- [0003-tenant-expand.md](./0003-tenant-expand.md): the previous phase. This ADR supersedes its `tenant-migration-expand` seeder command and its open question about the backfill mechanism.


----------------------------------------------------------------------------------------------------------------------------------------------------------------


# ADR 0003: Tenant Migration — Contract Phase

## Status:
Accepted

## Date:
2026-11-17

## Context

The [Expand](./0003-tenant-expand.md) and [Backfill](./0003-tenant-backfill.md) phases left the database in this state:

- Every tenant-scoped inventory table has a `TenantId` column, and no row has `NULL` in it.
- The legacy tenant, its admin user and its membership exist in the `Auth` schema.
- `TenantId` is still nullable and has no foreign key, so the database does not yet guarantee that a row belongs to a real tenant.
- `TenantContext` is a stub: its `TenantId` throws and `IsAvailable` is `false`. Nothing writes `TenantId`, so once the column becomes `NOT NULL` every insert would fail, and so would most integration tests.

The Contract phase enforces the invariant at the schema level. It must also let the Infrastructure layer set `TenantId` on every write without breaking the API or the integration tests, even though tenant resolution (M4 in ADR 0002) has not been built.

## Decision

### 1. `TenantContext` returns the legacy tenant

`SmartInventory.API.Services.TenantContext` is now a fixed, temporary implementation of `ITenantContext`:

| Member | Value |
|---|---|
| `TenantId` | `SystemTenants.LegacyTenantId` |
| `IsAvailable` | `true` |
| `IsTenantScoped` | `true` |

It is still registered as `Scoped`. When M4 adds real tenant resolution, only this class is replaced; nothing that consumes `ITenantContext` has to change.

### 2. `TenantId` is stamped in the Infrastructure layer

`SmartInventoryDbContext` accepts an optional `ITenantContext` (together with the optional `IMediator`). In `SaveChangesAsync`, when the context is present, available and tenant-scoped, it sets `TenantId` from `ITenantContext` on every tracked `ITenantEntity` that is `Added` or `Modified`.

- Handlers, controllers and DTOs do not know about tenants and do not send or receive a `TenantId`.
- This is the write-stamping part of ADR 0002 M5, delivered early and in its simplest form. Query filters and cross-tenant write rejection are **not** part of this phase.
- When no tenant context is supplied (the Seeds console app, design-time factory), nothing is stamped. In that case the seed data must carry its own `TenantId`, which the seed JSON files already do.

### 3. `TenantId` becomes mandatory

- `ITenantEntity.TenantId` changes from `Guid?` to `Guid`, on all eight entities (`Product`, `Warehouse`, `Stock`, `StockTransaction`, `Supplier`, `PurchaseOrder`, `PurchaseOrderItem`, `StockAlert`). Each EF configuration now uses `IsRequired()`.

### 4. Migration `20261117141904_TenantContract` (Inventory context)

The migration does the following, in this order for each table:

1. Drops the old global unique indexes.
2. Changes `TenantId` to `NOT NULL` (`AlterColumn`).
3. Creates the tenant-scoped replacements.
4. Adds a foreign key to `Auth.Tenants(Id)` with `ReferentialAction.NoAction`.

The unique indexes become tenant-scoped, as planned in ADR 0002 §2.5:

| Table | Old index | New index |
|---|---|---|
| `Products` | `UK_Products_SKU` | `UK_Products_Tenant_SKU` (`TenantId`, `SKU`) |
| `Warehouses` | `UK_Warehouses_Code` | `UK_Warehouses_Tenant_Code` (`TenantId`, `Code`) |
| `Suppliers` | `UK_Suppliers_Code` | `UK_Suppliers_Tenant_Code` (`TenantId`, `Code`) |
| `Stock` | `UK_Stock_Product_Warehouse` | `UK_Stock_Tenant_Product_Warehouse` (`TenantId`, `ProductId`, `WarehouseId`) |
| `StockTransactions` | `UK_StockTransactions_Number` | `UK_StockTransactions_Tenant_TransactionNumber` (`TenantId`, `TransactionNumber`) |
| `PurchaseOrders` | `UK_PurchaseOrders_Number` | `UK_PurchaseOrders_Tenant_OrderNumber` (`TenantId`, `OrderNumber`) |
| `PurchaseOrderItems` | `UK_PurchaseOrderItems_PO_Product` | `UK_PurchaseOrderItems_Tenant_PO_Product` (`TenantId`, `PurchaseOrderId`, `ProductId`) |

For `StockAlerts`, the migration adds two non-unique covering indexes, `IX_StockAlerts_Tenant_ProductId` and `IX_StockAlerts_Tenant_WarehouseId`, which include `Status`, `Severity` and `CreatedAt`. The old single-column `ProductId` and `WarehouseId` indexes are dropped and re-created.

- `Down()` is fully implemented. It drops the foreign keys, restores the old indexes and makes `TenantId` nullable again.
- The Auth migration `20261117141926_TenantContract` is intentionally empty. It exists only because it was scaffolded while the model snapshot was updated.

### 5. Foreign keys without navigation properties

As in ADR 0002 §2.4, inventory entities have no navigation property to `Tenant`, because `Tenant` belongs to `AuthDbContext`. The foreign keys are declared in the migration with `AddForeignKey` (`FK_<Table>_Tenants_TenantId`) and are not part of the inventory EF model. Both contexts share one physical database, so SQL Server enforces them.

### 6. Tests

- Integration tests build `SmartInventoryDbContext` with a default `TenantContext` (`CreateSmartInventoryDbContext()` in the base class), so test data is stamped with the legacy tenant.
- The fixture migrates `AuthDbContext` before `SmartInventoryDbContext`, because the foreign keys point to `Auth.Tenants`.
- The per-test cleanup skips the `Auth` schema and `__EFMigrationsHistory`, so the legacy tenant inserted by the Backfill migration survives between tests.
- `TenantMigrationTests` migrates Auth first as well.

### Out of scope

- Tenant resolution from the authenticated user: JWT, membership validation and tenant switching (M4).
- EF global query filters and rejecting cross-tenant writes (M5).
- Tenant-aware cache, S3 and background jobs (M6).
- Changes to the Application and API layers other than the `TenantContext` stub. They will be decided with M4.

## Alternatives

| Option | Why not |
|---|---|
| **Clients send `TenantId` in each request** | The caller would choose its own tenant, which defeats isolation. It would also change every DTO and handler, and the work would be discarded when real resolution is built. |
| **Handlers set the legacy tenant explicitly** | Business logic would depend on a migration artefact, and any handler that forgot the line would fail at the foreign key. |
| **Keep `TenantId` nullable and add only the foreign key** | A nullable foreign key does not stop rows with no tenant, so the invariant is not enforced. |
| **Navigation property from inventory entities to `Tenant`** | Cross-context navigations are not supported cleanly in EF Core (ADR 0002). |
| **Build full tenant resolution (M4) before Contract** | It makes the migration branch much larger, and the migration pattern itself would not be validated sooner. |

## Consequences

### Benefits

- The database now guarantees that every inventory row has an existing tenant, and uniqueness is scoped per tenant.
- No change in the API contract and no change in handlers.
- The stamping logic and the `ITenantContext` abstraction are the ones M4 and M5 need, so this is not throwaway work.
- The migration is fully reversible on the Inventory side.

### Trade-offs

- All data belongs to the legacy tenant, so the API is effectively single-tenant until M4.
- `TenantContext` hard-codes a tenant and lives in the API project, although the ADR scopes the change to Infrastructure.
- Without a tenant context, `SmartInventoryDbContext` does not stamp or validate anything.
- `NoAction` on the foreign keys means a tenant cannot be deleted while it still owns inventory data.

### Risks

- **Bypassing `SaveChangesAsync`.** Raw SQL or bulk operations are not stamped.
- **Late failure on non-empty databases.** `AlterColumn` uses `Guid.Empty` as its default. If any `NULL` row remains after Backfill, it becomes `Guid.Empty` and the foreign key then fails. Backfill must be applied first and verified.
- **Hard-coded tenant.** `TenantContext` always returns the legacy tenant. It must not reach a multi-tenant production environment.

## Definition of Done

- [X] All unit and integration tests pass.
- [X] The seeder app loads data into a clean database without errors.
- [X] The foreign key constraints on `TenantId` are in place and enforced.
- [X] The API applies the Auth migrations before the Inventory ones.

## Related ADRs

- [0002-tenancy-model.md](./0002-tenancy-model.md): the isolation model, the soft foreign key rule and the per-tenant uniqueness plan implemented here.
- [0003-tenant-expand.md](./0003-tenant-expand.md): added the nullable `TenantId` column.
- [0003-tenant-backfill.md](./0003-tenant-backfill.md): created the legacy tenant and assigned existing rows to it.


