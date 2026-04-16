# Smart Inventory Management System - Architecture Overview

## Project Information

**Project Name:** Smart Inventory Management System  
**Technology Stack:** .NET 10, C# 13, ASP.NET Core (Controllers + API Versioning)  
**Database:** SQL Server (two DbContexts)  
**Caching:** Microsoft Garnet via StackExchange.Redis  
**File Storage:** AWS S3 (via AWSSDK.S3 + Polly resilience)  
**Orchestration:** .NET Aspire (local development)  
**Timeline:** 6 weeks  
**Purpose:** Portfolio POC demonstrating modern .NET backend practices and integration with AI agents

---

## Architecture Style

### Pragmatic Clean Architecture with Vertical Slices

This project implements a **Pragmatic Clean Architecture** approach — combining Clean Architecture's layer separation with practical shortcuts that avoid unnecessary abstraction. Rather than traditional Repository and Unit of Work patterns, the Application layer accesses EF Core directly through thin DbContext interfaces, keeping the codebase lean and reducing boilerplate.

Combined with **Vertical Slice Architecture**, each feature is self-contained with its own Command/Query, Handler, Validator, and DTO, making it easy to add new features without impacting existing code.

**Core Principles:**
- **Direct DbContext access** from Application layer via interfaces (`IApplicationDbContext`, `IAuthDbContext`)
- **No Repository or Unit of Work abstractions** — EF Core's `DbContext` already implements both patterns
- **Clear separation of concerns** for maintainability and testability
- **Vertical slices** to prevent over-engineering and speed up feature development
- **Domain-centric design** with business logic isolated from infrastructure
- **Flexibility** to add features without impacting existing code

### Benefits for This Project
- Easy to explain and defend in technical interviews
- Demonstrates understanding of both traditional and modern approaches
- Reduces boilerplate code by eliminating unnecessary abstractions
- Testable at every layer (mock the DbContext interface, not a repository)
- Allows incremental development over 6 weeks
- Scales well if requirements grow

### Why Pragmatic Clean Architecture?
Traditional Clean Architecture often introduces Repository and Unit of Work abstractions that duplicate what EF Core already provides. This project follows the **Pragmatic Clean Architecture** philosophy:

1. **EF Core's `DbContext` is already a Unit of Work** — `SaveChangesAsync()` commits all tracked changes in a single transaction
2. **EF Core's `DbSet<T>` is already a Repository** — it provides `Add`, `Remove`, `Find`, and full LINQ query support
3. **Thin interfaces (`IApplicationDbContext`, `IAuthDbContext`)** maintain testability and the Dependency Inversion Principle without redundant wrappers
4. **Handlers query and persist directly** through the DbContext interface, eliminating an entire layer of pass-through code

---

## Solution Structure

```
SmartInventory/
│
├── src/
│   ├── SmartInventory.API/
│   │   ├── Controllers/
│   │   │   ├── V1/                 # Versioned API controllers
│   │   │   │   ├── ProductsController.cs
│   │   │   │   ├── WarehousesController.cs
│   │   │   │   └── LoginController.cs
│   │   │   └── VersionsController.cs
│   │   ├── Extensions/             # Extension helpers
│   │   │   └── ValidationResultExtensions.cs
│   │   ├── HealthChecks/           # Custom health check implementations
│   │   │   ├── MemoryHealthCheck.cs
│   │   │   ├── MemoryCheckOptions.cs
│   │   │   ├── DiskHealthCheck.cs
│   │   │   └── DiskCheckOptions.cs
│   │   ├── Mappings/               # Mapster configuration (API ↔ Contract mappings)
│   │   │   └── MappingConfig.cs
│   │   ├── Middleware/             # Global exception handler
│   │   │   └── GlobalExceptionHandler.cs
│   │   ├── Services/               # API-level service implementations
│   │   │   ├── CurrentUserService.cs
│   │   │   ├── ILinkService.cs
│   │   │   └── LinkService.cs
│   │   ├── Settings/               # Strongly-typed API configuration models
│   │   │   ├── AspnetcoreSettings.cs
│   │   │   └── CorsSettings.cs
│   │   ├── Validators/             # FluentValidation validators for API request models
│   │   │   ├── CreateProductRequestValidator.cs
│   │   │   ├── CreateWarehouseRequestValidator.cs
│   │   │   ├── FormFileValidator.cs
│   │   │   ├── GetUploadUrlRequestValidator.cs
│   │   │   ├── LoginRequestValidator.cs
│   │   │   └── UploadProductRequestValidator.cs
│   │   ├── Properties/             # Launch settings
│   │   ├── Logs/                   # Serilog file sink output (runtime-generated)
│   │   ├── ApplicationMapping.cs   # Endpoint mapping (health checks, OpenAPI, Scalar)
│   │   ├── DependencyInjection.cs  # API-layer service registrations
│   │   ├── KestrelConfiguration.cs # Kestrel server configuration
│   │   └── Program.cs              # Application entry point & DI composition root
│   │
│   ├── SmartInventory.Application/
│   │   ├── Features/               # Vertical slices by feature
│   │   │   ├── Auth/
│   │   │   │   └── Commands/       # LoginCommand, LoginCommandHandler, LoginCommandValidator
│   │   │   ├── Products/
│   │   │   │   ├── Commands/       # CreateProduct, DeleteProduct, GetUploadUrl, UploadProduct
│   │   │   │   │                   # (Command, Handler, Validator per operation)
│   │   │   │   ├── Queries/        # GetAllProducts, GetProductById (Query, Handler)
│   │   │   │   └── DTO/            # ProductDto
│   │   │   ├── Stocks/
│   │   │   │   ├── Queries/        # GetStockByProductId (Query, Handler)
│   │   │   │   └── DTO/            # StockDto
│   │   │   └── Warehouses/
│   │   │       ├── Commands/       # CreateWarehouse, DeleteWarehouse (Command, Handler, Validator)
│   │   │       ├── Queries/        # GetAllWarehouses, GetWarehouseById (Query, Handler)
│   │   │       └── DTO/            # WarehouseDto
│   │   ├── Common/
│   │   │   ├── Behaviors/          # MediatR pipeline behaviors
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── UnitOfWorkBehavior.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Cache/              # ICacheService interface, CacheKeys
│   │   │   ├── Exceptions/         # EntityNotFoundException, ValidationException
│   │   │   ├── Interfaces/         # IApplicationDbContext, IAuthDbContext, IJwtTokenService,
│   │   │   │                       # IFileStorageService, ICurrentUserService, ISensitiveRequest,
│   │   │   │                       # ICommand, IQuery, ICommandHandler, IQueryHandler
│   │   │   └── Models/             # PagedResult<T>
│   │   └── DependencyInjection.cs  # Application-layer service registrations
│   │
│   ├── SmartInventory.Domain/
│   │   ├── Entities/               # Product, Warehouse, Stock, StockTransaction,
│   │   │                           # Supplier, PurchaseOrder, PurchaseOrderItem,
│   │   │                           # StockAlert, Address
│   │   ├── Enums/                  # ProductCategory, UnitOfMeasure, WarehouseType,
│   │   │                           # TransactionType, PurchaseOrderStatus,
│   │   │                           # StockAlertType, AlertSeverity, AlertStatus
│   │   ├── Events/                 # ProductReorderPointReachedEvent, PurchaseOrderReceivedEvent,
│   │   │                           # StockLevelChangedEvent, StockTransactionCreatedEvent
│   │   ├── Exceptions/             # DuplicateEntityException, InsufficientStockException,
│   │   │                           # InvalidStockOperationException, ProductNotFoundException
│   │   ├── Identity/               # User, Role, UserRole
│   │   └── Interfaces/             # (reserved for domain service interfaces)
│   │
│   ├── SmartInventory.Infrastructure/
│   │   ├── Auth/                   # JWT token generation
│   │   │   └── JwtTokenService.cs
│   │   ├── BackgroundJobs/         # Quartz.NET job implementations
│   │   │   └── LowStockCheckJob.cs
│   │   ├── Data/
│   │   │   ├── Context/            # SmartInventoryDbContext, AuthDbContext,
│   │   │   │                       # Design-time factories, FactorySettings
│   │   │   ├── Configurations/     # EF Core entity configurations (per entity)
│   │   │   ├── Cache/              # GarnetCacheService (ICacheService implementation)
│   │   │   └── Repositories/       # (empty — direct DbContext access via interfaces)
│   │   ├── Extensions/             # IServiceCollection extension methods
│   │   │   ├── DBContextExtensions.cs
│   │   │   └── QuartzExtensions.cs
│   │   ├── Settings/               # Strongly-typed configuration models
│   │   │   └── JwtSettings.cs
│   │   ├── Migrations/
│   │   │   ├── Auth/               # Auth schema migrations
│   │   │   └── Inventory/          # Inventory schema migrations
│   │   └── DependencyInjection.cs  # Infrastructure-layer service registrations
│   │
│   ├── SmartInventory.Infrastructure.AWS/
│   │   ├── Storage/                # AWS S3 file storage implementation
│   │   │   └── S3FileStorageService.cs  # IFileStorageService implementation
│   │   ├── Settings/               # AWS configuration models
│   │   │   └── AwsSettings.cs
│   │   ├── PollyPolicies.cs        # Resilience & retry policies (Polly)
│   │   └── DependencyInjection.cs  # AWS-layer service registrations
│   │
│   ├── SmartInventory.AspireAppHost/
│   │   └── AppHost.cs              # .NET Aspire orchestration entry point
│   │
│   └── SmartInventory.Contracts/
│       ├── Requests/               # API request models (by feature)
│       │   ├── Login/              # LoginRequest
│       │   ├── Products/           # CreateProductRequest, GetUploadUrlRequest,
│       │   │                       # UploadProductRequest
│       │   ├── Warehouses/         # CreateWarehouseRequest
│       │   └── GetPagingRequest.cs # Shared pagination request model
│       └── Responses/              # API response models (by feature)
│           ├── Auth/               # LoginResponse
│           ├── Products/           # ProductResponse
│           ├── Warehouses/         # WarehouseResponse
│           ├── Link.cs             # HATEOAS link model
│           └── PagedResponse.cs    # Generic paged response wrapper
│
├── seeds/
│   └── SmartInventory.Seeds/       # Console app for database seeding
│       └── *Seeder.cs              # Per-entity seeders (Product, Stock, Warehouse, etc.)
│
├── tests/
│   ├── SmartInventory.UnitTests/
│   └── SmartInventory.IntegrationTests/
│
├── scripts/
│   ├── database/                   # SQL scripts (schema.sql)
│   └── docker/
│       └── garnet/                 # Docker Compose for Microsoft Garnet
│
└── docs/
    ├── architecture/               # Architecture documents
    ├── api/                        # API documentation
    └── setup/                      # Setup and deployment guides
```

---

## Layer Responsibilities

### 1. SmartInventory.API (Presentation Layer)

**Purpose:** HTTP interface, request/response mapping, and DI composition root

**Responsibilities:**
- Define versioned API controllers (V1, V2, etc.)
- Handle HTTP requests/responses
- Map between Contracts (request/response models) and Application DTOs using **Mapster**
- API versioning (`Asp.Versioning`)
- Exception handling middleware (`GlobalExceptionHandler`)
- Health checks( "/health", "/health/ready","/health/live" )
- OpenAPI documentation
- Serilog request logging
- HATEOAS link generation (`ILinkService` / `LinkService`)
- Current user resolution (`CurrentUserService` implementing `ICurrentUserService`)
- Kestrel server configuration (`KestrelConfiguration`)
- CORS and ASP.NET Core settings (`CorsSettings`, `AspnetcoreSettings`)
- Compose the DI container (register DbContexts, MediatR, validators, cache, etc.)

**Dependencies:** Application, Infrastructure, Infrastructure.AWS, Contracts, Domain

**Key Technologies:**
- ASP.NET Core Controllers with `Asp.Versioning`
- **Mapster** (API ↔ Contract mapping configuration in `MappingConfig.cs`)
- Serilog (structured logging)
- OpenAPI (`Microsoft.AspNetCore.OpenApi`)

---

### 2. SmartInventory.Application (Application Layer)

**Purpose:** Business logic orchestration and use cases via CQRS handlers

**Responsibilities:**
- Implement use cases/features as vertical slices
- Access database **directly** through `IApplicationDbContext` and `IAuthDbContext` interfaces
- DTO mapping using **Mapster** (`Adapt<T>()` in query handlers)
- Request validation (FluentValidation via `ValidationBehavior` pipeline)
- CQRS command/query handlers (MediatR)
- Define application-level interfaces (`ICacheService`, `IApplicationDbContext`, `IAuthDbContext`,
  `IFileStorageService`, `ICurrentUserService`, `ISensitiveRequest`)
- Application exceptions (`EntityNotFoundException`, `ValidationException`)
- Pagination support via `PagedResult<T>`

**Dependencies:** Domain, Contracts

**Key Patterns:**
- **Mediator Pattern** (MediatR) — handlers never call each other; all orchestration is via `IMediator`
- **CQRS-lite** — Commands for writes, Queries for reads, separate handler types (`ICommandHandler<,>`, `IQueryHandler<,>`)
- **Pipeline Behaviors** — ordered pipeline with `LoggingBehavior`, `ValidationBehavior`, and `UnitOfWorkBehavior`

**Key Technologies:**
- MediatR 14.x
- **Mapster 10.x** (DTO mapping in handlers)
- FluentValidation 12.x
- Microsoft.EntityFrameworkCore (for `DbSet<T>` access via interfaces)

**MediatR Pipeline (in order):**
1. `LoggingBehavior` — logs handler execution time and request details (skips `ISensitiveRequest`)
2. `ValidationBehavior` — runs FluentValidation rules; throws `ValidationException` on failure
3. `UnitOfWorkBehavior` — wraps commands in a transaction via `IUnitOfWork`

**Data Access Pattern:**
```csharp
// Handlers inject IApplicationDbContext directly — no repository layer
public class CreateProductCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = new Product { /* map from command */ };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return product.Id;
    }
}
```

---

### 3. SmartInventory.Domain (Domain Layer)

**Purpose:** Core business logic, entities, and domain rules

**Responsibilities:**
- Define domain entities (Product, Warehouse, Stock, etc.)
- Define identity entities (User, Role, UserRole)
- Business rules and invariants
- Domain events (StockLevelChangedEvent, etc.)
- Value objects (Address)
- Domain exceptions (InsufficientStockException, etc.)
- Domain enumerations
- Rich domain model with encapsulated behavior

**Dependencies:** None (pure domain — zero NuGet references)

**Key Patterns:**
- **Entity Pattern**
- **Value Object Pattern**
- **Domain Events Pattern**
- **Aggregate Root Pattern**

**Core Entities:**
- Product, Warehouse, Stock, StockTransaction
- Supplier, PurchaseOrder, PurchaseOrderItem
- StockAlert, Address (value object)

**Identity Entities:**
- User, Role, UserRole

---

### 4. SmartInventory.Infrastructure (Infrastructure Layer)

**Purpose:** External concerns, database implementations, and caching

**Responsibilities:**
- **Two EF Core DbContexts:**
  - `SmartInventoryDbContext` — implements `IApplicationDbContext` (Products, Stock, Warehouses, Suppliers, PurchaseOrders, etc.)
  - `AuthDbContext` — implements `IAuthDbContext` (Users, Roles, UserRoles)
- Entity configurations (Fluent API, one per entity)
- Design-time context factories for migrations
- Caching implementation (`GarnetCacheService` via StackExchange.Redis)
- EF Core migrations (separate folders for Auth and Inventory schemas)

**Dependencies:** Application, Domain

**Key Technologies:**
- Entity Framework Core 10 (SQL Server provider)
- StackExchange.Redis 2.x (connecting to Microsoft Garnet)

**Key Design Decisions:**
- **No Repository classes** — the `Repositories/` folder exists but is intentionally empty; all data access flows through DbContext interfaces
- **Marker interfaces for configuration isolation** — `IInventoryConfiguration` and `IAuthConfiguration` ensure each DbContext only applies its own entity configurations
- **Design-time factories** (`SmartInventoryDbContextFactory`, `AuthDbContextFactory`) enable `dotnet ef migrations` from the command line

---

### 5. SmartInventory.Contracts (Contracts Layer)

**Purpose:** Shared API request/response models

**Responsibilities:**
- API request models (organized by feature, e.g., `Requests/Products/`, `Requests/Warehouses/`)
- API response models (organized by feature, e.g., `Responses/Products/`, `Responses/Warehouses/`)
- Shared pagination request (`GetPagingRequest`) and response (`PagedResponse<T>`) models
- HATEOAS link model (`Link`)
- Shared between API and Application layers

**Dependencies:** None (pure models — zero NuGet references)

---

### 6. SmartInventory.Infrastructure.AWS (AWS Infrastructure Layer)

**Purpose:** AWS cloud service integrations (file storage)

**Responsibilities:**
- AWS S3 file upload/download via `S3FileStorageService` (implements `IFileStorageService`)
- Strongly-typed AWS configuration (`AwsSettings`)
- Resilience policies with **Polly** (`PollyPolicies`)
- DI registration for all AWS services

**Dependencies:** Application

**Key Technologies:**
- AWSSDK.S3
- Polly (resilience and transient fault handling)

---

### 7. SmartInventory.AspireAppHost (.NET Aspire Orchestration)

**Purpose:** Local development orchestration using .NET Aspire

**Responsibilities:**
- Declare and wire up all application resources (API, databases, cache) via `AppHost.cs`
- Provides the Aspire Dashboard for service discovery, telemetry, and health at development time

**Dependencies:** SmartInventory.API (project reference for Aspire wiring)

**Key Technologies:**
- .NET Aspire (`Aspire.Hosting`)

---

## Database Architecture

### Dual DbContext Strategy

The system uses **two separate DbContexts** to enforce a clear separation between business data and identity/authentication data:

```
┌──────────────────────────────────┐     ┌──────────────────────────────┐
│  IApplicationDbContext           │     │  IAuthDbContext               │
│  (SmartInventoryDbContext)       │     │  (AuthDbContext)              │
│──────────────────────────────────│     │──────────────────────────────│
│  Products                        │     │  Users                       │
│  Stocks                          │     │  Roles                       │
│  Warehouses                      │     │  UserRoles                   │
│  StockTransactions               │     └──────────────────────────────┘
│  Suppliers                       │
│  PurchaseOrders                  │
│  PurchaseOrderItems              │
└──────────────────────────────────┘
```

**Configuration Isolation:**
Each DbContext uses a marker interface to load only its relevant entity configurations:
- `SmartInventoryDbContext` loads configurations implementing `IInventoryConfiguration`
- `AuthDbContext` loads configurations implementing `IAuthConfiguration`

**Migrations:**
Separate migration folders under `Infrastructure/Migrations/`:
- `Migrations/Auth/` — Auth schema changes
- `Migrations/Inventory/` — Inventory schema changes

---

## Design Patterns Implemented

### 1. Pragmatic Clean Architecture (Direct DbContext Access)
**Purpose:** Eliminate unnecessary repository abstractions while maintaining testability  
**Location:** Application layer → Infrastructure layer  
**How:** Handlers inject `IApplicationDbContext` or `IAuthDbContext` and use `DbSet<T>` directly  
**Why:** EF Core's `DbContext` already implements Repository + Unit of Work; wrapping it adds complexity without value

### 2. Mediator Pattern
**Purpose:** Decouple request/response handling  
**Location:** Application layer  
**Library:** MediatR 14.x  
**Example:** `CreateProductCommand` → `CreateProductCommandHandler`

### 3. CQRS (Command Query Responsibility Segregation)
**Purpose:** Separate read and write operations with dedicated handler types  
**Location:** Application layer  
**Implementation:** `ICommand<T>` / `ICommandHandler<,>` for writes; `IQuery<T>` / `IQueryHandler<,>` for reads

### 4. Pipeline Behavior Pattern
**Purpose:** Cross-cutting concerns applied transparently to all MediatR requests  
**Location:** Application layer  
**Behaviors (in order):**
1. `LoggingBehavior<,>` — logs request name, duration, and outcome (skips `ISensitiveRequest`)
2. `ValidationBehavior<,>` — runs FluentValidation before every handler
3. `UnitOfWorkBehavior<,>` — wraps command handlers in a transaction (`IUnitOfWork`)

### 5. Strategy Pattern
**Purpose:** Interchangeable algorithms (inventory valuation — future)  
**Location:** Domain/Application layer  
**Example:** `IStockValuationStrategy` with FIFO, LIFO, WeightedAverage implementations

### 6. Specification Pattern
**Purpose:** Encapsulate business rules for queries (future)  
**Location:** Domain/Application layer  
**Example:** `LowStockSpecification`, `ActiveProductsSpecification`

---

## Technology Stack Details

### Core Framework
- **.NET 10** — Latest framework version
- **C# 13** — Latest language features (primary constructors, etc.)
- **ASP.NET Core Controllers** — With `Asp.Versioning` for versioned APIs

### API Versioning
- **Asp.Versioning.Http** + **Asp.Versioning.Mvc.ApiExplorer** 8.x
- URL segment versioning (`/api/v1/products`) as primary
- Header versioning (`X-Api-Version`) as fallback
- One OpenAPI document per version

### Database & ORM
- **SQL Server** (2019+) — Relational database
- **Entity Framework Core 10** — Primary ORM, accessed directly via DbContext interfaces
- **Two DbContexts:** `SmartInventoryDbContext` (business data) and `AuthDbContext` (identity data)

### Caching
- **Microsoft Garnet** — Redis-compatible cache server (runs via Docker)
- **StackExchange.Redis 2.x** — Client library connecting to Garnet
- **`ICacheService`** interface in Application layer, `GarnetCacheService` implementation in Infrastructure
- Configurable default TTL via `appsettings.json` (`Cache:DefaultTTLSeconds`)

### Validation & Mapping
- **FluentValidation 12.x** — Request validation via MediatR pipeline behavior
- **Mapster 10.x** — High-performance DTO mapping (replaces AutoMapper)
  - Used in query handlers (`products.Adapt<List<ProductDto>>()`)
  - Configured in API layer (`MappingConfig.RegisterMappings()`) for API ↔ Contract mappings

### File Storage
- **AWS S3** — Cloud object storage for product image uploads
- **AWSSDK.S3** — Official AWS SDK for S3 operations
- **Polly** — Resilience and retry policies for AWS SDK calls
- `IFileStorageService` interface in Application layer, `S3FileStorageService` implementation in `Infrastructure.AWS`
- Pre-signed URL support via `GetUploadUrl` command

### Orchestration & Developer Experience
- **.NET Aspire** — Local development orchestration and dashboard (`SmartInventory.AspireAppHost`)
  - Service discovery, structured logs, and health monitoring during development

### Logging & Monitoring
- **Serilog** — Structured logging
  - Serilog.AspNetCore 10.x
  - Serilog.Sinks.Console — Console output with custom template
  - Serilog.Sinks.File — Rolling daily file logs (`Logs/apilog-*.txt`)
  - Serilog.Enrichers.Environment — Machine name enrichment
  - Serilog.Settings.Configuration — Config-driven setup

### Authentication & Security
- **JWT Bearer Authentication** — 
- **BCrypt.Net** or **Identity.PasswordHasher** — 
- **AuthDbContext** with User/Role/UserRole entities ready

### API Documentation
- **Microsoft.AspNetCore.OpenApi** — OpenAPI document generation
- Per-version OpenAPI documents (`/openapi/v1.json`)

### Testing
- **xUnit** — Testing framework
- **FluentAssertions** — Readable test assertions
- Unit tests and integration tests (mapping, math, etc.)

### Containerization
- **Docker** — API Dockerfile (Linux target)
- **Docker Compose** — Microsoft Garnet cache server

---

## Data Flow

### Command Flow (Write Operations)

```
1. API Controller receives HTTP request
   ↓
2. Controller maps Contract request → MediatR Command (Mapster)
   ↓
3. Command sent via IMediator.Send()
   ↓
4. LoggingBehavior records start time and request details
   ↓
5. ValidationBehavior runs FluentValidation rules
   ↓
6. UnitOfWorkBehavior opens a transaction
   ↓
7. CommandHandler accesses IApplicationDbContext directly
   ↓
8. Domain entities created/modified (business rules enforced)
   ↓
9. DbContext.SaveChangesAsync() persists changes (committed by UnitOfWorkBehavior)
   ↓
10. Domain events published (if any)
    ↓
11. Response returned to controller → mapped to Contract response
```

### Query Flow (Read Operations)

```
1. API Controller receives HTTP request
   ↓
2. Query sent via IMediator.Send()
   ↓
3. LoggingBehavior records start time
   ↓
4. QueryHandler checks ICacheService (Garnet) for cached data
   ↓
5. If cache miss:
   - Query IApplicationDbContext directly (AsNoTracking for reads)
   - Map entities → DTOs (Mapster)
   - Store in cache
   ↓
6. Return DTO → Controller maps to Contract response (with HATEOAS links if applicable)
```

### File Upload Flow

```
1. Client requests a pre-signed upload URL
   ↓
2. GetUploadUrlCommand → GetUploadUrlCommandHandler
   ↓
3. IFileStorageService.GeneratePresignedUrl() (S3FileStorageService with Polly retry)
   ↓
4. Pre-signed URL returned to client
   ↓
5. Client uploads file directly to AWS S3 using pre-signed URL
   ↓
6. Client calls UploadProductCommand with the S3 object key
   ↓
7. Product.ImageUrl updated in database
```

---

## Cross-Cutting Concerns

### 1. Exception Handling
- `GlobalExceptionHandler` middleware (`IExceptionHandler` implementation)
- Domain-specific exceptions (InsufficientStockException, etc.)
- Application exceptions (EntityNotFoundException, ValidationException)
- Standardized ProblemDetails responses

### 2. Validation
- Request validation via FluentValidation + `ValidationBehavior` MediatR pipeline
- Domain entity validation within entity constructors/methods
- Separate validators per command (e.g., `CreateProductCommandValidator`)

### 3. Logging
- Structured logging with Serilog
- Request/response logging via `UseSerilogRequestLogging()`
- Console sink for development
- Rolling file sink for persistence
- Machine name enrichment for multi-instance scenarios

### 4. Caching Strategy
- **Microsoft Garnet** as Redis-compatible cache server (containerized)
- **Cache-Aside Pattern** for read operations via `ICacheService`
- Configurable TTL (default 300 seconds)
- JSON serialization for cached values

### 5. Authentication & Authorization 
- JWT token-based authentication
- Role-based authorization
- Dedicated `AuthDbContext` with User, Role, UserRole entities
- Token refresh mechanism (Planned)
- Secure password storage

### 6. Background Jobs (Quartz.NET)
- **Low Stock Check Job** — Runs every hour
- **Stock Snapshot Job** — Runs daily at midnight
- **Reorder Suggestion Job** — Runs daily at 8 AM
- Job execution logging and error handling

---

## Integration Points

### Internal Integration
All API endpoints communicate via:
- REST APIs with JSON
- Versioned endpoints (`/api/v1/...`)
- Standardized error handling (ProblemDetails)
- OpenAPI documentation per version

### External Integration (Future AI Agents)
**Webhook Endpoints:**
- Stock level change notifications
- Low stock alerts
- Reorder triggers

**Data Export APIs:**
- Historical stock data for forecasting
- Product demand patterns
- Supplier performance metrics

**Integration Format:**
- JSON over HTTP/HTTPS
- Event-driven notifications
- Batch data exports

---

## Security Considerations

### Authentication
- JWT tokens with expiration (planned)
- Refresh token mechanism
- Secure token storage guidelines
- Dedicated `AuthDbContext` for identity data isolation

### Authorization
- Role-based access control (RBAC)
- Endpoint-level authorization
- Resource-level permissions

### Data Protection
- Sensitive data encryption at rest
- HTTPS only (TLS 1.2+)
- SQL injection prevention (parameterized queries via EF Core)
- Input validation and sanitization (FluentValidation)

### API Security
- Rate limiting per client
- CORS configuration
- API versioning for backward compatibility

---

## Scalability Considerations

### Current Design
- Stateless API (horizontal scaling ready)
- Distributed caching (Microsoft Garnet via StackExchange.Redis)
- Separate DbContexts for business and identity data
- Database connection pooling

### Future Enhancements
- Background job processing (Quartz.NET)
- Read replicas for reporting
- CQRS with separate read/write databases
- Event sourcing for audit trail
- Microservices split (if needed)
- Message queue (RabbitMQ/Azure Service Bus)
- Dapper for high-performance complex reports

---

## Development Workflow

### 1. Feature Development
1. Define domain entity (if new)
2. Add `DbSet<T>` to relevant DbContext interface and implementation
3. Create EF Core entity configuration
4. Create command/query with handler (vertical slice)
5. Add FluentValidation rules
6. Define API controller endpoint
7. Configure Mapster mappings (if API ↔ Contract mapping needed)
8. Write unit tests
9. Write integration tests

### 2. Testing Strategy
- **Unit Tests:** Domain logic, handlers, validators, mapping
- **Integration Tests:** Database operations, API endpoints, mapping integration

### 3. Database Migrations
- Code-first approach with EF Core
- Separate migration paths for Auth and Inventory DbContexts
- Seed data via dedicated `SmartInventory.Seeds` console app (JSON-based)

---

## Deployment Considerations

### Environments
- **Development:** Local SQL Server, Garnet via Docker Compose, .NET Aspire dashboard (`SmartInventory.AspireAppHost`)
- **Staging:** Azure SQL, Azure Cache for Redis, AWS S3
- **Production:** Azure SQL with geo-replication, Redis cluster, AWS S3

### Containerization
- Docker support (Linux target) for API
- Docker Compose for Microsoft Garnet (local development)
- Kubernetes-ready (if needed)

### CI/CD Pipeline
- Automated builds
- Unit and integration tests
- Code quality checks
- Automated deployments

---

## Success Metrics

### Technical Metrics
- API response time < 200ms (95th percentile)
- Test coverage > 80%
- Zero critical security vulnerabilities
- All endpoints documented

### Interview Readiness
- Can explain architecture decisions (especially Pragmatic Clean Architecture trade-offs)
- Can discuss why Repository/UoW was intentionally omitted
- Can demonstrate working features
- Can explain scalability path
- Can discuss .NET 10 features used

---

## Documentation Structure

### API Documentation
- OpenAPI specification per version (`/openapi/v1.json`)
- Request/response examples
- Authentication guide (planned)

### Architecture Documentation
- This overview document
- Database schema design
- Domain model documentation

### Developer Documentation
- Setup instructions
- Development guidelines
- Testing strategy
- Deployment guide

---

## Timeline Alignment

**Week 1-2:** Foundation (API, Application, Domain, Infrastructure, dual DbContexts, caching)  
**Week 3-4:** Core features (Products, Stock, PurchaseOrders) and seeding  
**Week 5:** Performance optimization (caching, testing)  
**Week 6:** Auth, polish, documentation, deployment

---

**Document Version:** 3.0  
**Last Updated:** April 2026  
**Author: Jose Valdes, Senior .NET Developer, Bolivia