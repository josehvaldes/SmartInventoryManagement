# ADR 0002: Multi-Tenant Isolation Model (Pool with Shared Database + TenantId)

## Status:
Accepted

## Date:
2026-11-14

## Context

[ADR 0001](./0001-testing-and-engineering-readiness.md) deferred the tenancy decision to this record. The engineering safety net (Testcontainers integration tests, CI, characterization suite, startup decoupling, fail-closed identity) is in place, so we can now commit to an isolation strategy without losing the ability to detect regressions or cross-tenant leaks.

The product is being migrated from single-tenant to multi-tenant SaaS. The migration roadmap is split into milestones; this ADR makes the **strategic isolation decision** and pins the **M2 tenant foundation** (`Tenant`, `TenantMembership`, `ITenantContext`). 


**Current single-tenant state** (the touch points the refactor will hit, taken from ADR 0001 and confirmed against the codebase):

- No entity has a `TenantId`. [`BaseEntity`](../../src/SmartInventory.Domain/Entities/BaseEntity.cs) only carries domain events, and [`User`](../../src/SmartInventory.Domain/Identity/User.cs) is global.
- Uniqueness is global: `UK_Products_SKU`, `UK_Warehouses_Code` and `UK_Stock_Product_Warehouse` are defined in the inventory schema; `StockTransaction.TransactionNumber`, `Supplier.Code` and `PurchaseOrder.OrderNumber` are also globally unique by design.
- [`CacheKeys<T>.ById(id)`](../../src/SmartInventory.Application/Common/Cache/CacheKeys.cs) has no tenant segment.
- S3 object keys are `products/{guid}-{file}` ([`S3FileStorageService`](../../src/SmartInventory.Infrastructure.AWS/Storage/S3FileStorageService.cs)).
- The JWT carries no tenant claim.
- Two DbContexts share one SQL Server database: `SmartInventoryDbContext` (inventory) and `AuthDbContext` (identity). Both implement thin interfaces (`IApplicationDbContext`, `IAuthDbContext`) consumed directly by handlers — no repository layer.
- [`CurrentUserService`](../../src/SmartInventory.API/Services/CurrentUserService.cs) was already hardened in ADR 0001 to fail closed for anything beyond cosmetic audit values.
- Quartz jobs (currently scaffolded: `LowStockCheckJob`) are not tenant-aware.
- EF migrations are applied explicitly (no longer inline in `Program.cs` thanks to ADR 0001's startup decoupling), which gives the tenancy work a clean place for per-tenant provisioning.

The first deliverable after this ADR is **M2 — Tenant foundation**: introduce `Tenant`, `TenantMembership`, and an `ITenantContext` abstraction that the rest of the system can resolve against. Nothing else in M3–M8 can be designed sensibly until M2 exists and its shape is fixed, so this ADR commits to that shape.

## Decision

### 1. Isolation strategy: Pool model — shared database, shared schema, `TenantId` discriminator

We adopt a **pool isolation model**: one SQL Server database, one shared schema, with a `TenantId` column on every tenant-scoped entity. Isolation is enforced primarily by **EF Core global query filters** that scope every read to the current tenant, plus a `SaveChangesAsync` interceptor that stamps and validates `TenantId` on writes. There is no per-tenant schema and no per-tenant database in this phase.

This is the foundation milestones M2–M8 build on. M9 (Tenant Registry + database-per-tenant routing) is an explicit evolution path; if we ever adopt it, it will be a new ADR superseding parts of this one — not an extension.

### 2. M2 deliverables: tenant foundation

The following are committed as part of this ADR. They go in **Phase 1 of the migration** and are the prerequisite for every later milestone.

#### 2.1 `Tenant` aggregate (Domain layer)

New aggregate root in `SmartInventory.Domain/Entities/Tenant.cs`:

```csharp
public class Tenant : BaseEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }          // display name, max 200
    public string Slug { get; private set; }          // URL-safe identifier, globally unique, max 50
    public TenantStatus Status { get; private set; } // Active | Suspended | Provisioning
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Domain methods: Activate(), Suspend(), Rename(), etc.
}
```

- `Slug` is the externally visible identifier (used in URLs / subdomain / `Origin` header in M4) and is globally unique. `Id` is the internal FK used by every tenant-scoped entity.
- `TenantStatus` enum lives in `SmartInventory.Domain/Enums/`. `Suspended` tenants get blocked at the resolution layer (M4), not at the data layer.
- No billing/plan/feature-flags fields yet — premature. They will be added when needed, behind their own decision.

#### 2.2 `TenantMembership` entity (Domain layer)

Lives in `SmartInventory.Domain/Identity/TenantMembership.cs` and links `User` to `Tenant` with a tenant-scoped role:

```csharp
public class TenantMembership : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantRole Role { get; private set; }  // Admin | Manager | Member
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; }
}
```

- Unique constraint: `(TenantId, UserId)` — a user can be a member of a tenant at most once.
- `TenantRole` is **tenant-scoped** and is distinct from the existing global `Role`/`UserRole` (which governs platform-level concerns like a future SaaS operator role). A user can be `Admin` in tenant A and `Member` in tenant B.
- A user may belong to multiple tenants. Tenant switching (and the JWT claim shape that supports it) is decided in ADR 0003, not here.

#### 2.3 `ITenantContext` abstraction (Application layer)

Mirrors the `ICurrentUserService` pattern already in the codebase. New file `SmartInventory.Application/Common/Interfaces/ITenantContext.cs`:

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsAvailable { get; }   // false for system/admin/health-check paths
    bool IsTenantScoped { get; } // false when explicitly operating cross-tenant (operator/system)
}
```

- Resolved by middleware added in ADR 0003. Until ADR 0003 ships, the default implementation returns `IsAvailable = false` and the rest of the system treats that as "no tenant context" — exactly the current single-tenant behaviour.
- **Fail-closed**: handlers that require a tenant and find `IsAvailable == false` throw, mirroring ADR 0001's hardening of `CurrentUserService`. We do not default to a sentinel tenant.
- Scoped as a request-scoped service (`Scoped` lifetime), same as `ICurrentUserService`.

#### 2.4 Where `Tenant` lives: `AuthDbContext`

`Tenant` and `TenantMembership` are added to `AuthDbContext` (not `SmartInventoryDbContext`) because they are identity concepts. `IAuthDbContext` gains `DbSet<Tenant>` and `DbSet<TenantMembership>`, with entity configurations implementing the existing `IAuthConfiguration` marker.

`TenantId` on inventory entities (Product, Warehouse, Stock, StockTransaction, Supplier, PurchaseOrder, StockAlert — added in M3) is a **non-navigation FK**: an explicit `Guid TenantId` property, with a database-level FK constraint defined in the inventory migration, but **no navigation property back to `Tenant`** from inventory entities. This preserves the existing two-DbContext split and avoids EF cross-context navigation, which is poorly supported. The two DbContexts already share one physical database, so the FK constraint is real and enforced by SQL Server.

#### 2.5 Per-tenant uniqueness — committed shape (detailed migration in ADR 0004)

Globally-unique indexes become **composite unique indexes scoped by `TenantId`**. The committed list (final names decided in ADR 0004):

| Current index                       | New index                                       |
|-------------------------------------|-------------------------------------------------|
| `UK_Products_SKU`                   | `UK_Products_Tenant_SKU` (`TenantId`, `SKU`)     |
| `UK_Warehouses_Code`                | `UK_Warehouses_Tenant_Code` (`TenantId`, `Code`) |
| `UK_Stock_Product_Warehouse`        | `UK_Stock_Tenant_Product_Warehouse`             |
| `StockTransaction.TransactionNumber` | unique per `TenantId` (sequence or app-generated) |
| `Supplier.Code`                     | `UK_Suppliers_Tenant_Code`                       |
| `PurchaseOrder.OrderNumber`         | unique per `TenantId`                            |

Auto-generated identifiers (`TransactionNumber`, `OrderNumber`) will use **per-tenant sequences** rather than a global counter, so two tenants can both have `TXN-000001` without colliding. The exact mechanism (SQL Server sequences vs. app-side `TenantId`-prefixed format) is decided in ADR 0004.

### 3. Roadmap — milestones M3–M8 → planned ADRs

The remaining milestones are scoped here only as **pointers**, so each future ADR has a clear remit. No detailed decisions are made for them in this ADR.

| Milestone | Planned ADR | Scope of that ADR |
|---|---|---|
| M3 — Domain migration (`ITenantEntity`, `TenantId` column, indexes, relationships) | ADR 0004 (planned) | Concrete migration: which entities get `TenantId`, the `ITenantEntity` marker interface, the index changes listed in §2.5, data backfill strategy for the existing single-tenant rows. |
| M4 — Tenant resolution (auth, membership validation, middleware) | ADR 0003 (planned) | JWT claim shape (`tenant_id` hint + DB validation vs. DB-only), middleware order, `ITenantContext` populator, tenant switching endpoint, `Suspended` tenant handling. |
| M5 — Data isolation (EF filters + write enforcement) | ADR 0004 (planned, shared with M3) | Global query filter on `ITenantEntity`, `SaveChangesAsync` interceptor that stamps `TenantId` on new entities and rejects cross-tenant writes, admin/system bypass path. |
| M6 — Infrastructure isolation (cache, S3, background jobs) | ADR 0005 (planned) | `CacheKeys<T>` tenant segment, S3 key prefix `tenants/{tenantId}/products/{guid}-{file}`, Quartz job scoping (iterate tenants as the outer loop). |
| M7 — Tenant provisioning | ADR 0006 (planned) | Create-tenant flow, initial admin invitation, default seed data per tenant, idempotency, the provisioning runner (reuses ADR 0001's startup decoupling). |
| M8 — Cross-tenant isolation verification | ADR 0007 (planned) | Integration test suite built on the ADR 0001 Testcontainers fixture: cross-tenant read/write attempts, uniqueness boundary tests, suspended-tenant tests. |
| M9 — Tenant Registry + DB-per-tenant routing | **Deferred. Not pre-decided.** | If/when we adopt DB-per-tenant for specific tenants (e.g. enterprise tier), a new ADR will supersede the relevant parts of this one. |

### 4. Out of scope

- **Tenant billing, plans, feature flags, usage quotas.** Add when there is a real billing requirement; do not pre-build.
- **Per-tenant customization** (custom fields, per-tenant business rules). Not decided here.
- **Database-per-tenant or hybrid isolation** (M9). Explicitly deferred; revisiting it will produce a new ADR that supersedes parts of this one.
- **Tenant onboarding UX** (signup flow, email verification, billing capture). Covered by ADR 0006 (provisioning) when we get to M7.
- **Cross-tenant admin/operator role model.** The existing global `Role`/`UserRole` continues to mean "platform-level"; we are not designing a SaaS operator console here.

## Alternatives

| Option | Why not (for now) |
|---|---|
| **Schema-per-tenant** (shared DB, per-tenant schema, e.g. `tenant_abc.Products`) | Stronger isolation, but EF Core has poor native support for runtime schema switching — we would need a `ModelCacheKeyFactory` and per-request `DbContext` configuration, which complicates the dual-DbContext design we already have. Migration story is painful: every schema change must be replicated N times. Pool model is sufficient at our scale and far simpler. |
| **Database-per-tenant** | Strongest isolation, easiest to reason about, simplest to reason about backups/restore per tenant. But: connection-pool fragmentation, cross-tenant queries (future analytics, operator console) become hard, provisioning latency is higher, and operational overhead (N databases to migrate, monitor, back up) grows linearly with tenants. Deferred to M9 for the enterprise-tier case if it ever materializes; pool covers the default. |
| **Separate app instance per tenant** | Operationally simplest isolation, but worst cost-per-tenant economics and no shared code path. We would also lose the single-deploy SaaS story we are building towards. |
| **Hybrid: some entities isolated, others shared** | Sounds pragmatic, but it forces every handler to know "is this entity tenant-scoped or global?", which leaks the isolation model into business logic. We pick one rule: **every tenant-facing domain entity is tenant-scoped**, with `User`, `Tenant`, `TenantMembership` as the only global identity entities. |
| **Tenant resolved from URL path vs. subdomain vs. header vs. JWT claim only** | This is a tenant-resolution decision, deferred to ADR 0003 (M4). ADR 0002 only commits to the data model; resolution mechanism is independent. |
| **Cross-context navigation property from inventory entities back to `Tenant`** | EF Core does not support navigation properties across `DbContext`s cleanly. A soft FK (column + DB-level constraint, no EF navigation) is the standard pattern and preserves the existing two-context split. |

## Consequences

### Benefits

- **One code path, one schema, one migration.** Pool isolation keeps the existing dual-DbContext design and the existing migration workflow intact; we add a column and a filter, not a parallel schema.
- **Cheapest path to multi-tenancy at our scale.** A handful of tenants (portfolio scope, then early SaaS) is exactly the regime where pool wins on cost and operational simplicity.
- **`Tenant` foundation is reusable.** `Tenant`, `TenantMembership` and `ITenantContext` are the right primitives regardless of whether a specific tenant later moves to its own DB (M9). They survive the evolution.
- **Isolation is enforceable in one place.** EF Core global query filters + a `SaveChangesAsync` interceptor concentrate the isolation logic in the data layer, instead of scattering `Where(t => t.TenantId == ...)` across every handler.
- **Testable with the existing fixture.** The Testcontainers integration fixture from ADR 0001 already spins up real SQL Server and Garnet; the cross-tenant isolation tests (M8) reuse it without changes.
- **Clear path to DB-per-tenant later.** Pool + `TenantId` discriminator is the standard stepping stone; M9's Tenant Registry can route specific tenants to dedicated databases without rewriting business logic, because handlers never see the routing decision.

### Trade-offs

- **Noisy-neighbour risk.** A single tenant with a runaway query or a huge data load can degrade everyone. Mitigation: query timeouts, per-tenant rate limiting (already in the stack), and the option to move that tenant to its own DB in M9.
- **EF Core global query filters can be bypassed** with `IgnoreQueryFilters()`. Mitigation: the write-side interceptor enforces `TenantId` on writes, and the M8 test suite explicitly probes for filter-bypass attempts. `IgnoreQueryFilters()` calls will be flagged in code review.
- **Migration of existing data.** The current single-tenant rows need a `TenantId` backfill. This is a one-time operation in M3 (ADR 0004), scoped to a default tenant that the existing data is assigned to.
- **Per-tenant uniqueness is enforced at the DB layer**, which means migrations for unique-index changes must be careful (drop old, backfill `TenantId`, create new). ADR 0004 will detail this.
- **Two DbContexts sharing a database with a cross-context FK** is slightly unusual. It works because the two contexts already share one physical database, but the FK is not modelled in EF — it exists only at the SQL schema level. Documented in §2.4.
- **Auto-generated sequences become per-tenant.** `TransactionNumber` and `OrderNumber` lose global ordering. Acceptable: they were never meaningful across tenants.

### Risks

- **A missed `IgnoreQueryFilters()` or a forgotten `TenantId` stamp leaks data across tenants.** This is the highest-severity risk. Mitigations: the M8 cross-tenant test suite is mandatory before any tenancy code ships to `main`; the write-side interceptor fails closed; code review checklist includes "is this query going through a filter-eligible `DbSet`?".
- **`ITenantContext` becomes an implicit dependency** that handlers forget to inject, then silently operate without tenant scoping. Mitigation: the interceptor also fails closed when `IsAvailable == false` for tenant-scoped entities, so the failure mode is loud (throws) not silent (returns wrong data).
- **JWT-claim staleness** if the resolution mechanism (ADR 0003) puts `tenant_id` in the JWT and a user is removed from a tenant mid-session. Mitigation: defer to ADR 0003; one option is JWT hint + DB validation on each request, cached briefly.
- **M9 may force a partial rewrite.** If we adopt DB-per-tenant for some tenants later, the EF Core global query filter model still works but the connection-string resolution needs to be added. This is acceptable and expected — that is exactly what M9 is for, and the pool model is chosen so the rewrite is contained to the data layer.

### Definition of done (for M2 only)

- `Tenant` and `TenantMembership` aggregates exist in `SmartInventory.Domain`, with domain methods, invariants, and unit tests.
- `TenantStatus` and `TenantRole` enums exist in `SmartInventory.Domain/Enums/`.
- `ITenantContext` interface exists in `SmartInventory.Application/Common/Interfaces/`, with a default no-op implementation registered as `Scoped`.
- `AuthDbContext` exposes `DbSet<Tenant>` and `DbSet<TenantMembership>`; configurations implement `IAuthConfiguration`; migrations are added.
- `IAuthDbContext` interface updated; handlers can already inject it (no behaviour change yet).
- The existing single-tenant characterization tests from ADR 0001 still pass — no inventory entity has changed yet.
- ADR 0003 (tenant resolution) and ADR 0004 (domain migration + data isolation) are stubbed in `docs/adr/` with status `Proposed` and a one-paragraph "this ADR will decide …" placeholder.

## Related ADRs

- [0000-template.md](./0000-template.md): the template this ADR follows.
- [0001-testing-and-engineering-readiness.md](./0001-testing-and-engineering-readiness.md): the engineering-readiness prerequisite. Its Phase 4 explicitly defers the tenancy decision to this record.
- ADR 0003 (planned, M4): tenant resolution pipeline.
- ADR 0004 (planned, M3 + M5): domain migration and data-isolation enforcement.
- ADR 0005 (planned, M6): tenant-aware cache, S3 and Quartz jobs.
- ADR 0006 (planned, M7): tenant provisioning.
- ADR 0007 (planned, M8): cross-tenant isolation verification suite.
- ADR 0008 (not planned yet, M9): database-per-tenant routing. Will supersede parts of this ADR if/when adopted.

## Implementation details

- `Tenant.Slug` is the externally visible identifier; `Tenant.Id` is the internal FK. Never expose `Id` in URLs; never use `Slug` as a foreign key.
- The unique constraint on `Tenant.Slug` is global (one slug per tenant across the whole platform).
- `TenantMembership` unique constraint is `(TenantId, UserId)`. Soft-deletes (`IsActive = false`) preserve the row so the same user can be re-invited without recreating a row.
- `ITenantContext` is registered as `Scoped`, mirroring `ICurrentUserService`. Until M4 ships, the default implementation returns `IsAvailable = false` and handlers treat that as the current single-tenant behaviour (no tenant filter active).
- `Tenant` and `TenantMembership` go in `AuthDbContext`. Their EF configurations implement `IAuthConfiguration`. Inventory entities that need `TenantId` (added in M3) will define the FK as a column with a DB-level constraint, no EF navigation property — this is the standard cross-DbContext soft-FK pattern.
- The `User` entity gains no navigation property to `TenantMembership` in `AuthDbContext` for now; resolution happens through the membership table query in M4. If query performance demands it later, a navigation property can be added inside `AuthDbContext` only (same context, no cross-context issue).
- `TenantRole` enum values are `Admin = 1`, `Manager = 2`, `Member = 3`. Mirrors the existing enum-numbering convention used in `SmartInventory.Domain/Enums/`.
- No `Tenant` audit fields (`CreatedBy`, `UpdatedBy`) yet — `Tenant` is created by the provisioning flow (M7 / ADR 0006), not by an HTTP request from a logged-in user. We add audit fields in ADR 0006 if needed.
- The existing `BaseEntity` only carries domain events; it does **not** gain a `TenantId`. Tenant scoping is opt-in via a new `ITenantEntity` marker interface introduced in M3 (ADR 0004). This keeps non-tenant-scoped global entities (e.g. lookup tables if any are added later) explicitly outside the filter.
