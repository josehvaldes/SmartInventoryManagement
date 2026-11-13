# ADR 0001: Testing & Engineering Readiness Before the Multi-Tenant Refactor 

## Status: 
Accepted

## Date: 2026-11-09 

## Context
We plan to turn the Smart Inventory portfolio app into a **multi-tenant SaaS**. That refactor touches the data model, auth, caching, storage and background jobs at once, so we first need a safety net and a few engineering foundations. The current state:

**Testing**
- [MappingIntegrationTests.cs](../../tests/SmartInventory.IntegrationTests/MappingIntegrationTests.cs) is the only integration test. It uses a hard-coded `Server=SERVERPC;...` connection string and expects pre-seeded data, so it fails on any other machine and in CI.
- Unit tests for query handlers use `MockDbSetHelper` plus a custom async query provider. They do not exercise real SQL translation, so they miss provider-specific behaviour the schema relies on: `NEWSEQUENTIALID()`, the persisted computed column `Stock.QuantityAvailable`, check constraints and unique indexes (`UK_Products_SKU`, `UK_Stock_Product_Warehouse`). EF InMemory/SQLite cannot validate these either.
- No HTTP-level tests cover the pipeline (auth, validation, `GlobalExceptionHandler`, MediatR behaviors, Unit of Work). The integration test project has no `Microsoft.NET.Test.Sdk`, `Microsoft.AspNetCore.Mvc.Testing` or reference to the API.
- There is no CI workflow. `.github/` only contains the Codacy instructions file.
- There is not OpenAPI specification for future frontend testing integration.

**Engineering hygiene**
- [Program.cs](../../src/SmartInventory.API/Program.cs) runs `MigrateAsync()` and `DatabaseSeeder.SeedAllAsync()` inline under `IsDevelopment()`, and the API project references the Seeds project. This couples app startup to migration/seeding and makes a "Testing" environment awkward.
- [CurrentUserService](../../src/SmartInventory.API/Services/CurrentUserService.cs) returns `"Unknown"` when there is no user. For audit fields that is tolerable, but for tenant resolution it would fail open.
- `FluentAssertions` 8.x and `MediatR` (`LicenseKey` in [DependencyInjection.cs](../../src/SmartInventory.Application/DependencyInjection.cs)) have commercial-licensing terms that matter once the product is a commercial SaaS.

**Tenant-isolation touch points the refactor will hit** (inventory only, not decided here):
- No entity has a `TenantId`. `BaseEntity` only carries domain events, and `User` is global.
- Uniqueness is global: `UK_Products_SKU` and warehouse/stock keys.
- `CacheKeys<T>.ById(id)` has no tenant segment.
- S3 keys are `products/{guid}-{file}`.
- The JWT has no tenant claim.
- There are two DbContexts (Inventory, Auth) on one connection string.
- Quartz jobs are not tenant-aware.

The multi-tenancy refactor will change behaviour across all of these. Without a trustworthy integration suite, we cannot tell whether we broke existing behaviour or leaked data across tenants.

## Decision
Before any tenancy work starts, we will make the following improvements in four phases. Each phase is independently mergeable.

### Phase 1: Testcontainers-based integration tests (first item)
- Add **Testcontainers** for .NET (`Testcontainers.MsSql`, plus Redis/Garnet) so integration tests run against disposable, real dependencies. Use the same SQL Server and Garnet images as the Aspire AppHost and `scripts/docker/garnet`, with **pinned tags**.
- Add `Microsoft.NET.Test.Sdk`, `Microsoft.AspNetCore.Mvc.Testing` and `Respawn` to `SmartInventory.IntegrationTests`, and reference the API project.
- Expose the entry point with `public partial class Program { }` so `WebApplicationFactory<Program>` works.
- Start containers **once per test run** with an xUnit v3 assembly/collection fixture. The `WebApplicationFactory` overrides `ConnectionStrings:SmartInventoryDb`, the `redis` connection string and `JwtSettings`, and runs in a `Testing` environment. The fixture applies EF migrations for both `SmartInventoryDbContext` and `AuthDbContext` explicitly, and the Development-only migrate/seed block in `Program.cs` stays untouched.
- Reset data between tests with **Respawn** (ignoring `__EFMigrationsHistory`) and seed through small test-data builders instead of the full `DatabaseSeeder`.
- Replace `IFileStorageService` (S3) with a test double for Unit tests AND LocalStack for Integration Tests with TestContainers.
- Remove or Rewrite `MappingIntegrationTests` on the new fixture (no `SERVERPC`).
- Document the Docker prerequisite in `docs/setup/`.
- Add a OpenAPI generator integrated in the build process.

### Phase 2: Characterization test suite
Add HTTP-level integration tests that pin **current single-tenant behaviour** for login/refresh/revoke, products (including SKU uniqueness and paging), warehouses, and stock removal (including the computed `QuantityAvailable` and check constraints). This suite is the regression net for the tenancy refactor. Keep fast, pure-logic unit tests (validators, domain rules). Migrate `MockDbSetHelper`-based handler tests to integration tests opportunistically, not in one big bang.

### Phase 3: Engineering hygiene
1. **CI**: add a GitHub Actions workflow (`ubuntu-latest`, which has Docker) that runs build, unit tests and Testcontainers integration tests, with coverage via `coverlet.collector`, on every PR.
2. **Startup decoupling**: move migrate/seed out of `Program.cs` into an explicit mechanism (a `--migrate` mode or a separate runner), and drop the API → Seeds project reference. This also gives the tenancy work a clean place for per-tenant provisioning.
3. **Fail closed on identity**: stop defaulting to `"Unknown"` in `CurrentUserService` for anything that is not a purely cosmetic audit value, and add a test that pins this.
4. **Licensing review**: decide on `FluentAssertions` 8.x (replace with `AwesomeAssertions`/`Shouldly` or accept the license) and on `MediatR`'s license key before the SaaS launch.
5. **Config/secrets**: keep connection strings and JWT secrets out of `appsettings*.json` for non-dev environments (env vars or a secret store).

### Phase 4: Add Observability Stack
1. Implement Prometheus metrics for the API. Basic metrics for Authentication errors and Poly's service unavailability are enought at this stage.
2. Clean console loggings, promttail add Loki containers.
3. Add Tempo container and OpenTelemetry dependencies and basic configuration in the API.

### Phase 5: Hand-off to the tenancy ADR
Once Phases 1-3 are green, write **ADR 0002 (tenancy model)**, covering the isolation strategy (shared DB with `TenantId` and EF global query filters vs. schema-per-tenant vs. DB-per-tenant), tenant resolution, per-tenant uniqueness, cache/storage/job scoping, and a **cross-tenant isolation test suite** built on the Phase 1 fixture. That decision is deliberately not made here.

### Out of scope
load/performance testing, contract testing, and any schema or behaviour change to the product itself.

## Alternatives
| Option | Why not (for now) |
|---|---|
| **EF Core InMemory** | Does not enforce constraints, computed columns, unique indexes or SQL translation, so it gives false confidence. |
| **SQLite in-memory** | Different SQL dialect. `NEWSEQUENTIALID()`, `SYSUTCDATETIME()`, persisted computed columns and schemas (`Inventory`, `Auth`) do not behave the same. |
| **Shared/local SQL Server instance** | Not reproducible or isolated, and cannot run in CI. This is the current `SERVERPC` problem. |
| **LocalDB** | Windows-only and not Linux CI friendly. |
| **Aspire.Hosting.Testing** (reuse `AppHost`) | Viable and closest to the real topology, but it is heavier and couples tests to the AppHost. We may revisit it for end-to-end tests. |
| **Hand-written docker-compose for tests** | Manual lifecycle and port management, and no automatic cleanup. Testcontainers gives programmatic, per-run lifecycle. |
| **Start tenancy now and add tests along the way** | Highest-risk path. Isolation bugs are data-leak bugs, and we would have no baseline for existing behaviour. |

## Consequences
**Benefits**
- Tests run against real SQL Server and Garnet, so schema-level behaviour is verified and the suite is portable across dev machines and CI.
- A regression net exists before the risky refactor, and the same fixture is reused for cross-tenant isolation tests.
- Startup is decoupled from migration/seeding, and CI gates every PR.
- Licensing and fail-open identity risks are found before they matter.
- A generated OpenAPI specification document will facilitated integration tests between the API and future frontend applications

**Trade-offs**
- **Docker is required** to run integration tests, including locally.
- Slower feedback: the SQL Server container takes tens of seconds to start. Mitigation: one container per run, Respawn instead of recreating the DB, and a clear unit/integration split so unit tests stay fast.
- More upfront work (a few days) before visible SaaS progress.
- Two test styles (mock-based and container-based) coexist during the migration.

**Risks**
- Flaky container startup in CI. Mitigation: pinned image tags, health/wait strategies, and retries on the CI step.
- Container image licensing (SQL Server EULA acceptance) and image pull limits in CI. Mitigation: pin tags and use a registry cache if needed.
- Test data drift from `DatabaseSeeder`. Mitigation: test-specific builders.

**Definition of done**
- `dotnet test` passes on a clean machine that only has Docker and the .NET SDK, with no manual DB setup.
- The CI workflow runs unit and integration tests on PRs.
- Characterization tests cover the auth, products, warehouses and stock endpoints.
- ADR 0002 can start from a green baseline.
- Prometheus Metrics are available in /metrics endpoint.


## Related ADRS
- [0000-template.md](./0000-template.md): the template this ADR follows.
- ADR 0002 (planned): multi-tenancy model and tenant isolation. It depends on this ADR.

## Implementation details.
- Bugs in API contracts will not be fixed, but new test cases are added and marked as "Skip" for a later bug-fixing stage
- Bugs that block the TestContainers will be fixed with High priority.
- FluentAssertions version was downgraded to 7.2.2 to avoid the Commercial-Licensing problem and use the apache license
- MediatR version was downgraded to 12.5.0 to avoid the Commercial-Licensing problem and use the apache license
- `MappingIntegrationTests` class was removed and replaced by TestContainer tests.