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
