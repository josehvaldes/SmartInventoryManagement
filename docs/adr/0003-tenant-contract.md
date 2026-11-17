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
