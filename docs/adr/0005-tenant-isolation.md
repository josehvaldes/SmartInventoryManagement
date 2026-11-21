# ADR 0005 Tenant Isolation (Data and Infrastructure)

## Status: 
Accepted

## Date: 
2026-11-20


## Context
After tenant resolution was done (ADR 0004), the API has to isolate read and write operations so that their scope is limited to the current tenant context.
Also, the cache service and the AWS S3 infrastructure must be tenant scoped.

## Decision
- Add a `HasQueryFilter` at the EF infrastructure level (`SmartInventoryDbContext`) using a generic method, so any current or future entity implementing `ITenantEntity` automatically gets the filter.
- Enforce the tenant scope in `SaveChangesAsync` for the `Added`, `Modified`, and `Deleted` states.
- `IsTenantScoped` is `true` by default. Future cross-tenant operations (operator/system) will update this property, but that is out of the scope of this ADR.
- Replace the static `CacheKeys<T>` class with an injectable `ICacheKeyFactory` that depends on `ITenantContext`, so every cache key includes the tenant id.
- Keep `IFileStorageService` as a Singleton, but pass the `ITenantContext` to the methods that interact with AWS S3.
- Currently the background jobs have no tenant-aware functionality, but `ITenantContext` is already in place for future use. Only placeholder (commented) parameters were added; no behavior changes in the background jobs.

## Alternatives
- **Manual `Where(x => x.TenantId == ...)` in each handler**: rejected, easy to forget and not enforced for new entities.
- **Database-level Row-Level Security (SQL Server `SESSION_CONTEXT` + security policy)**: stronger guarantee, but adds DB-specific complexity; may be considered later as defense in depth.
- **Keep static `CacheKeys<T>` and pass `tenantId` explicitly (`ByTenantAndId`)**: rejected, callers can still use the non-tenant overloads by mistake.
- **Make `IFileStorageService` Scoped and inject `ITenantContext` in its constructor**: rejected to keep the singleton S3 client wrapper stateless.

## Consequences
**Benefits**
- Tenant isolation is enforced centrally; handlers do not need to know about tenants for reads/writes.
- A cross-tenant lookup (e.g. another tenant's product id) returns *not found* instead of leaking data.
- Cache entries and S3 objects are partitioned per tenant.

**Trade-offs / risks**
- Filters and the ownership check only apply to `ITenantEntity` types going through `SmartInventoryDbContext`. `IgnoreQueryFilters()`, raw SQL, `ExecuteUpdate`/`ExecuteDelete` bypass them.
- `IFileStorageService` signature is a breaking change for every caller and test.
- Cross-tenant operations will need an explicit, audited opt-in (`IsTenantScoped = false`).

## Related ADRS
ADR 0002
ADR 0003
ADR 0004

## Implementation details

### EF Core (`SmartInventoryDbContext`)
- `OnModelCreating` iterates over all root entity types assignable to `ITenantEntity` (derived types inherit the filter) and invokes the generic `ApplyTenantFilter<TEntity>` through reflection.
- The filter is `IsTenantAvailable && x.TenantId == CurrentTenantId`. It **fails closed**: when no tenant is available, no rows are returned. The filter reads the properties of the context instance, so EF evaluates it per `DbContext` (per request) and does not cache a tenant id in the compiled model.
- `EnforceTenantOwnership()` runs at the start of `SaveChangesAsync`:
  - `Added`: throws `TenantException(ContextRequired, 401)` if no tenant is available; when `IsTenantScoped`, `TenantId` is **always overwritten** with the current tenant (a client-supplied value is ignored).
  - `Modified` / `Deleted`: throws `TenantException(Forbidden, 403)` if no tenant is available, or when `IsTenantScoped` and the entity's `TenantId` differs from the current tenant (also catches an attempt to change `TenantId`).
- New error code `TenantErrorCodes.Forbidden` (`tenant_forbidden_operation`) with a public message.
- `TenantContext.IsTenantScoped` now defaults to `true` (it was `false`), so isolation is the default and cross-tenant access must be explicit.

### Cache
- `CacheKeys<T>` was deleted. `ICacheKeyFactory` / `CacheKeyFactory` (registered as Scoped in `Application/DependencyInjection.cs`) build keys as:
  - `{Type}:Tenant:{tenantId}:Id:{id}` (`ById<T>`)
  - `{Type}:Tenant:{tenantId}:{name}:{property}` (`ByProperty<T>`)
- All Product, Warehouse, Stock and Tenant handlers now use the factory.
- `GetTenantMembershipsQueryHandler` no longer caches: its key depended on the request's tenant/user rather than on the current context, and the result is a short-lived (30s) authorization lookup.

### S3 (`S3FileStorageService`)
- `GetUploadUrlAsync` and `UploadAsync` receive `ITenantContext`. Keys are now `tenants/{tenantId}/products/{fileName}`; the call throws `InvalidOperationException` if the tenant is unavailable or empty.
- The handlers (`GetUploadUrlCommandHandler`, `UploadProductCommandHandler`) pass the tenant context.

### Tests
- Unit tests were updated for the new constructor signatures.
- Integration test data helpers were moved to `tests/SmartInventory.IntegrationTests/Data`, and `Tenants/TenantIsolation.cs` covers: get products with a different tenant returns empty, and delete product with a different tenant returns not found.



### Observations for later fixes
.
1. **Cache key when tenant is unavailable**: `CacheKeyFactory` silently produces `Tenant:00000000-...` keys (the guard `EnsureTenantAvailable` is commented out). Tenant resolution itself (`GetTenantBySlug`/`GetTenantById`) runs before the context is initialized, so enabling the guard would break it. Suggest: throw for tenant-owned types, and allow a documented "global" key for tenant-resolution lookups (e.g. a separate `ByGlobalProperty`), instead of sharing the empty-Guid bucket.

2. **Bypass paths**: `ExpiredRefreshTokenCleanupJob` uses `ExecuteDeleteAsync` (on `IAuthDbContext`, which is not tenant-filtered by design). Any future `ExecuteUpdate`/`ExecuteDelete` or `IgnoreQueryFilters()` on tenant entities must be reviewed; consider an analyzer/architecture test that forbids `IgnoreQueryFilters` outside an approved location.

3. **Defense in depth**: consider SQL Server Row-Level Security and a composite index starting with `TenantId` on tenant-owned tables to support the filter performance.
