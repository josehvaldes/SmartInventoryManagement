# Smart Inventory Management System

Inventory management API built with .NET 10, 'Pragmatic' Clean Architecture, Domain-Driven Design and CQRS-lite. It manages products across multiple warehouses and tracks stock levels. Purchase orders, stock transactions and low-stock alerts are in progress (see [Features](#features)).

> Portfolio project. The next milestone is turning it into a multi-tenant SaaS; see [docs/adr](docs/adr).

## Architecture

- **Pragmatic Clean Architecture:** `Domain` → `Application` → `Infrastructure` → `API`. Handlers use an `IApplicationDbContext` abstraction instead of repositories.
- **Vertical slices:** each feature folder in `Application` holds its commands, queries, validators and DTOs.
- **CQRS-lite with MediatR:** pipeline behaviors for logging, validation and unit of work.
- **DDD:** rich entities and domain events, with integration events published through an AWS SNS publisher.
- **Two DbContexts:** Inventory (`SmartInventoryDbContext`) and Auth (`AuthDbContext`) on one SQL Server database.

Design decisions are recorded as ADRs in [docs/adr](docs/adr). More detail is in [docs/architecture](docs/architecture).

## Tech Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10, ASP.NET Core versioned controllers (`Asp.Versioning`) |
| Data | SQL Server, EF Core 10 (code-first migrations) |
| Cache | Microsoft Garnet / Redis (`StackExchange.Redis`) |
| Jobs | Quartz.NET |
| Application | MediatR 12.5, FluentValidation 12, Mapster |
| Cloud | AWS S3 (pre-signed uploads), AWS SNS, Polly resilience policies |
| Security | JWT + refresh token cookie, role policies, rate limiting, CORS |
| Observability | Serilog, health checks (`/health`, `/health/ready`, `/health/live`) |
| API docs | OpenAPI + Scalar |
| Orchestration | .NET Aspire (SQL Server + Redis + API) |
| Testing | xUnit v3, FluentAssertions 7.2.2, NSubstitute, Testcontainers (SQL Server, Redis, LocalStack) |

## Features

| Area | Status |
|------|--------|
| Products: CRUD, paging, HATEOAS links, S3 image upload and pre-signed URLs | ✅ |
| Warehouses: CRUD, paging, address value object | ✅ |
| Stock: query by product, remove stock | ✅ |
| Auth: login, refresh and revoke token, role policies, rate-limited auth endpoints | ✅ |
| Low-stock check job (Quartz) | 🔲 scaffolded, logic pending |
| Stock transactions and alerts | 🔲 domain only |
| Suppliers and purchase orders | 🔲 domain only |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/). It is required for the integration tests, and it is the easiest way to run SQL Server and Garnet locally.
- An AWS S3 bucket, only if you use the file-upload features.

### 1. Clone

```powershell
git clone https://github.com/josehvaldes/SmartInventoryManagement.git
cd SmartInventoryManagement
```

### 2. Start dependencies

**Option A: Aspire** starts SQL Server, Redis and the API together:

```powershell
dotnet run --project src/SmartInventory.AspireAppHost
```

**Option B: Docker Compose** starts SQL Server and Garnet and runs the API directly:

```powershell
# Create scripts/docker/.env containing MSSQL_SA_PASSWORD=<strong-password>
docker compose -f scripts/docker/docker-compose-sqlserver.yml up -d
docker compose -f scripts/docker/docker-compose-garnet.yml up -d
```

Then set the settings below in `src/SmartInventory.API/appsettings.Development.json` (or user secrets or environment variables) and run:

```powershell
dotnet run --project src/SmartInventory.API
```

```json
{
  "ConnectionStrings": {
    "SmartInventoryDb": "Server=localhost,1433;Database=SmartInventoryDB;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True;",
    "redis": "localhost:6379"
  },
  "JwtSettings": { "Secret": "<secret>", "Issuer": "<issuer>", "Audience": "<audience>" },
  "AwsSettings": { "AccessKey": "<key>", "SecretKey": "<secret>", "Region": "us-east-1", "S3BucketName": "<bucket>" }
}
```

### 3. Migrations and seed data

Migrations are applied automatically on startup when the environment is `Development`. Seeding is a separate, explicit step:

```powershell
$env:ConnectionStrings__SmartInventoryDb = "<connection string>"
dotnet run --project seeds/SmartInventory.Seeds              # seed everything
dotnet run --project seeds/SmartInventory.Seeds ProductSeeder # or a single seeder
```

### 4. Explore

- API docs: `https://localhost:7134/scalar/v1` (ports are defined in `launchSettings.json`)
- Health: `/health`, `/health/ready` (SQL Server, Redis, memory, disk), `/health/live`
- Sample requests: [SmartInventory.API.http](src/SmartInventory.API/SmartInventory.API.http)

## Docker Image

Build the API image from the repository root with [scripts/builds/build-images.ps1](scripts/builds/build-images.ps1):

```powershell
.\scripts\builds\build-images.ps1                  # builds smartinventory-api:1.0
.\scripts\builds\build-images.ps1 -Version 1.2.0   # builds smartinventory-api:1.2.0
```

Run it by passing configuration through environment variables:

```powershell
docker run --rm -p 8080:8080 `
  -e ConnectionStrings__SmartInventoryDb="<connection string>" `
  -e ConnectionStrings__redis="<host>:6379" `
  -e JwtSettings__Secret="<secret>" -e JwtSettings__Issuer="<issuer>" -e JwtSettings__Audience="<audience>" `
  smartinventory-api:1.0
```

## Testing

```powershell
dotnet test tests/SmartInventory.UnitTests          # fast, no Docker needed
dotnet test tests/SmartInventory.IntegrationTests   # needs Docker running
dotnet test                                         # everything
```

Integration tests run the real API in-process (`WebApplicationFactory`) against disposable Testcontainers: SQL Server 2022, Redis 7.2 and LocalStack for S3. No manual database setup or connection strings are needed. The tests are described in [ADR 0001](docs/adr/0001-testing-and-engineering-readiness.md).

## CI/CD

GitHub Actions ([ci-smart-inventory-api.yml](.github/workflows/ci-smart-inventory-api.yml) calling [build-dotnet.yml](.github/workflows/build-dotnet.yml)) restores, builds and tests the solution on pull requests and on pushes to `main`. Pushes to `main` also publish the image to `ghcr.io/<owner>/smartinventory-api` (`latest` and `sha-<commit>` tags). Details: [docs/ci.md](docs/ci.md).

## Documentation

- [Architecture](docs/architecture/smart-inventory-architecture.md)
- [Domain model](docs/architecture/smart-inventory-domain.md)
- [Database schema](docs/architecture/smart-inventory-db-schema.md)
- [Architecture decision records](docs/adr)
- [CI/CD](docs/ci.md)
- [Development log](docs/DEVLOG.md)

## Roadmap

- ✅ Domain model, EF Core schema, JWT auth
- ✅ Products, warehouses and stock endpoints, S3 uploads, Aspire
- ✅ Testing and engineering readiness ([ADR 0001](docs/adr/0001-testing-and-engineering-readiness.md))
- 🔲 Multi-tenant SaaS refactor (analysis and ADR pending)
- 🔲 Stock transactions, purchase orders, alerts, full background jobs

## License

GPL-3.0. See [LICENSE](LICENSE).

## Author

Jose Valdes
