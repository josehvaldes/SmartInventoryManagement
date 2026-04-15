# Smart Inventory Management System - Cinnamon AI Agents

A modern, enterprise-grade inventory management system built with .NET 10, demonstrating 'Pragmatic' Clean Architecture principles, Domain-Driven Design, and modern backend development practices.

## 🎯 Project Overview

**Purpose:** Portfolio POC showcasing production-ready .NET development with AI integration capabilities  
**Timeline:** 6-week development cycle  
**Architecture:** 'Pragmatic' Clean Architecture with Vertical Slice flexibility

This system manages product inventory across multiple warehouses, handles purchase orders, tracks stock movements, and provides automated alerts for low stock conditions.

---

## 🏗️ Architecture

### Design Approach

The project implements a **hybrid architecture** combining:
- **Pragmatic Clean Architecture (DbContext abstraction)** for clear separation of concerns and testability
- **Vertical Slice Architecture** for feature-focused development
- **Domain-Driven Design (DDD)** with rich domain models
- **CQRS-lite** for separating read and write operations

### Key Benefits
✅ Highly maintainable and testable  
✅ Clear dependency flow (Domain → Application → Infrastructure → API)  
✅ Scalable for future enhancements  
✅ Production-ready patterns and practices  
✅ AI-agent integration ready

---

## 🛠️ Technology Stack

### Core Framework
- **.NET 10** - Latest framework
- **C# 13** - Modern language features
- **ASP.NET Core MVC Controllers** - Versioned REST API controllers

### Database & Data Access
- **SQL Server 2025** - Primary database
- **Entity Framework Core 10** - ORM with code-first migrations (two separate DbContexts: Inventory and Auth)

### Caching & Background Processing
- **Microsoft Garnet** - Redis-compatible distributed cache (via StackExchange.Redis client)
- **Quartz.NET** - Scheduled background jobs (e.g. low stock checks)

### Validation & Mapping
- **FluentValidation 11.x** - Request validation (pipeline behavior + controller-level)
- **Mapster** - High-performance object mapping with custom type adapter configurations

### Architecture Patterns & Libraries
- **MediatR** - Command/Query handling with pipeline behaviors (Logging, Validation, UnitOfWork)
- **Asp.Versioning** - URL-segment and header-based API versioning
- **HATEOAS** - Hypermedia links on resource responses via `ILinkService`
- **Polly** - Resilience and transient-fault-handling policies (retry + circuit breaker)

### Cloud & File Storage
- **AWS S3** - Product image storage with pre-signed upload URLs
- **Amazon SDK for .NET** - S3 client integration

### Orchestration
- **.NET Aspire** - Local development orchestration (SQL Server + Redis + API)

### Logging & Monitoring
- **Serilog** - Structured logging with request logging middleware
- **Health Checks** - SQL Server, Redis, memory, and disk health endpoints (`/health`, `/health/ready`, `/health/live`)
- **HealthChecks.UI** - Rich JSON health check responses

### API Documentation
- **Scalar** - Modern OpenAPI documentation UI

### Security
- **JWT Bearer Authentication** - Token-based auth with configurable issuer/audience/secret
- **ASP.NET Core Identity** - User and role management
- **Role-based Authorization** - `AdminOnly` and `ManagerOnly` policies
- **Rate Limiting** - Sliding window (global), fixed window (write ops), strict fixed window (auth endpoints)
- **CORS** - Configurable allowed origins per policy

### Testing
- **xUnit v3** - Testing framework
- **FluentAssertions** - Readable assertions
- **NSubstitute** - Mocking

---

## 📁 Solution Structure

```
SmartInventory/
│
├── src/
│   ├── SmartInventory.API/              # Presentation Layer
│   │   ├── Controllers/V1/              # Versioned MVC controllers (Products, Warehouses, Auth)
│   │   ├── Middleware/                  # Global exception handler
│   │   ├── Validators/                  # FluentValidation validators (API layer)
│   │   ├── HealthChecks/                # Memory and disk health checks
│   │   ├── Services/                    # ILinkService (HATEOAS), ICurrentUserService
│   │   ├── Mappings/                    # Mapster type adapter configuration
│   │   ├── Settings/                    # CORS and Aspnet core settings
│   │   └── Program.cs                   # Entry point + auto-migration + seeding (dev)
│   │
│   ├── SmartInventory.Application/      # Application Layer
│   │   ├── Features/                    # Vertical slices by feature
│   │   │   ├── Auth/                    # Login command
│   │   │   ├── Products/                # Create, Delete, GetById, GetAll, Upload, GetUploadUrl
│   │   │   ├── Warehouses/              # Create, Delete, GetById, GetAll
│   │   │   └── Stocks/                  # GetStockByProductId
│   │   ├── Common/
│   │   │   ├── Behaviors/               # MediatR pipelines: Logging, Validation, UnitOfWork
│   │   │   ├── Cache/                   # ICacheService abstraction + cache keys
│   │   │   ├── Exceptions/              # EntityNotFoundException, ValidationException
│   │   │   ├── Interfaces/              # IApplicationDbContext, IAuthDbContext, IJwtTokenService,
│   │   │   │                            # IFileStorageService, ICurrentUserService, ICommand/IQuery
│   │   │   └── Models/                  # PagedResult<T>
│   │   └── DependencyInjection.cs
│   │
│   ├── SmartInventory.Domain/           # Domain Layer (Core)
│   │   ├── Entities/                    # Product, Warehouse, Stock, StockTransaction,
│   │   │                                # Supplier, PurchaseOrder, PurchaseOrderItem, StockAlert
│   │   ├── Identity/                    # User, Role, UserRole (ASP.NET Core Identity)
│   │   ├── Enums/                       # ProductCategory, UnitOfMeasure, TransactionType,
│   │   │                                # PurchaseOrderStatus, AlertStatus, AlertSeverity, etc.
│   │   ├── Events/                      # Domain events (StockLevelChanged, ProductReorderPointReached, etc.)
│   │   └── Exceptions/                  # Domain-specific exceptions
│   │
│   ├── SmartInventory.Infrastructure/   # Infrastructure Layer (SQL Server + Garnet/Redis + Quartz)
│   │   ├── Data/
│   │   │   ├── Context/                 # SmartInventoryDbContext, AuthDbContext (+ factories)
│   │   │   ├── Configurations/          # EF Core entity type configurations
│   │   │   ├── Cache/                   # GarnetCacheService (StackExchange.Redis)
│   │   │   └── Migrations/              # EF Core migrations (Inventory + Auth schemas)
│   │   ├── Auth/                        # JwtTokenService
│   │   ├── BackgroundJobs/              # LowStockCheckJob (Quartz.NET)
│   │   ├── Extensions/                  # QuartzExtensions, DBContextExtensions
│   │   └── Settings/                    # JwtSettings
│   │
│   ├── SmartInventory.Infrastructure.AWS/  # AWS Infrastructure Layer
│   │   ├── Storage/                     # S3FileStorageService (upload + pre-signed URLs)
│   │   ├── Settings/                    # AwsSettings
│   │   └── PollyPolicies.cs             # Retry + circuit breaker policies
│   │
│   ├── SmartInventory.AspireAppHost/    # .NET Aspire Orchestration
│   │   └── AppHost.cs                   # Wires SQL Server, Redis, and API resources
│   │
│   └── SmartInventory.Contracts/        # Shared Request/Response DTOs
│       ├── Requests/                    # CreateProductRequest, CreateWarehouseRequest,
│       │                                # LoginRequest, UploadProductRequest, GetUploadUrlRequest
│       └── Responses/                   # ProductResponse, WarehouseResponse, LoginResponse,
│                                        # PagedResponse<T>, Link
│
├── seeds/
│   └── SmartInventory.Seeds/            # Database seeder (auto-run in Development)
│
├── tests/
│   ├── SmartInventory.UnitTests/        # Handler unit tests (Products, Warehouses, Stocks)
│   └── SmartInventory.IntegrationTests/ # Mapping integration tests
│
└── docs/
    ├── architecture/                     # Architecture documents
    ├── api/                             # API documentation
    └── setup/                           # Setup guides
```

---

## 🎯 Core Features

### Products
- ✅ Product catalog with SKU management
- ✅ Create, retrieve (by ID / paged list), and delete products
- ✅ Product image upload to AWS S3 (direct stream upload)
- ✅ Pre-signed S3 URL generation for client-side uploads
- ✅ Paginated product listing with HATEOAS links

### Warehouses
- ✅ Multi-warehouse support (Retail, Distribution, Cold Storage, etc.)
- ✅ Create, retrieve (by ID / paged list), and delete warehouses
- ✅ Address value object embedded in warehouse entity
- ✅ HATEOAS links on warehouse responses

### Stock
- ✅ Stock entity per product/warehouse combination
- ✅ Query stock levels by product ID
- ✅ Domain entities for StockTransaction and StockAlert defined

### Authentication & Authorization
- ✅ JWT login endpoint with FluentValidation
- ✅ Role-based authorization (`AdminOnly`, `ManagerOnly` policies)
- ✅ Rate-limited auth endpoint (5 req/min, no queuing)
- 🔲 Refresh token endpoint (placeholder — not fully implemented)

### Background Jobs
- ✅ Low stock check job wired with Quartz.NET (runs on cron schedule)
- 🔲 Full stock check logic (job scaffolded, business logic pending)
- 🔲 Daily stock snapshots
- 🔲 Reorder suggestions

### Purchase Orders & Suppliers
- 🔲 Supplier management (domain entities defined, API not yet implemented)
- 🔲 PO creation and approval workflow
- 🔲 Status tracking (Draft → Submitted → Confirmed → Received)

### Alerts & Notifications
- 🔲 Low stock alert workflow (domain entities defined, API not yet implemented)

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2025 Developer](https://www.microsoft.com/sql-server/sql-server-downloads)
- [Microsoft Garnet](https://microsoft.github.io/garnet/) or [Redis](https://redis.io/download) (optional for local development)
- AWS account with an S3 bucket (required for file upload features)
- IDE: Visual Studio 2026

### Installation

1. **Clone the repository**
   ```powershell
   git clone https://github.com/josehvaldes/SmartInventoryManagement.git
   cd SmartInventoryManagement
   ```

2. **Update connection strings and settings**

   Edit `src/SmartInventory.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "SmartInventoryDb": "Server=localhost;Database=SmartInventoryDB;Trusted_Connection=True;TrustServerCertificate=True;",
       "redis": "localhost:6379"
     },
     "JwtSettings": {
       "Secret": "<your-secret>",
       "Issuer": "<your-issuer>",
       "Audience": "<your-audience>"
     },
     "AwsSettings": {
       "AccessKey": "<your-access-key>",
       "SecretKey": "<your-secret-key>",
       "Region": "us-east-1",
       "S3BucketName": "<your-bucket>"
     }
   }
   ```

3. **Restore NuGet packages**
   ```powershell
   dotnet restore
   ```

4. **Run the application**

   **Option A – Direct run** (auto-migrates and seeds the DB in Development):
   ```powershell
   cd src/SmartInventory.API
   dotnet run
   ```

   **Option B – .NET Aspire** (orchestrates SQL Server, Redis, and the API together):
   ```powershell
   cd src/SmartInventory.AspireAppHost
   dotnet run
   ```

5. **Access API documentation**

   Open browser to: `https://localhost:<port>/scalar/v1`

6. **Health check endpoints**

   | Endpoint | Description |
   |----------|-------------|
   | `/health` | Overall health |
   | `/health/ready` | Readiness (SQL Server, Redis, memory, disk) |
   | `/health/live` | Liveness (self check) |

---

## 🧪 Testing

### Run Unit Tests
```powershell
dotnet test tests/SmartInventory.UnitTests
```

### Run Integration Tests
```powershell
dotnet test tests/SmartInventory.IntegrationTests
```

### Run All Tests
```powershell
dotnet test
```

> **Note:** Integration tests require a live SQL Server connection. Update the connection string in `MappingIntegrationTests.cs` before running.

---

## 📊 Database Schema

The system uses SQL Server with two separate EF Core DbContexts:

- **SmartInventoryDbContext** – Inventory schema: Products, Warehouses, Stock, StockTransactions, Suppliers, PurchaseOrders, PurchaseOrderItems, StockAlerts
- **AuthDbContext** – Identity schema: Users, Roles, UserRoles (ASP.NET Core Identity)

Migrations are applied automatically on startup in `Development` mode. The `SmartInventory.Seeds` project seeds initial data after migration.

See [Database Schema Documentation](docs/architecture/smart-inventory-db-schema.md) for detailed schema design.

---

## 🎨 Design Patterns

This project demonstrates the following design patterns:

| Pattern | Purpose | Location |
|---------|---------|----------|
| **CQRS** | Separate read/write operations via ICommand/IQuery | Application |
| **Mediator (MediatR)** | Decoupled request handling with pipeline behaviors | Application |
| **Unit of Work** | Transaction management via behavior | Application/Infrastructure |
| **Factory** | DbContext factory for design-time migrations | Infrastructure |
| **Chain of Responsibility** | MediatR pipeline: Logging → Validation → UnitOfWork | Application |
| **Decorator** | Polly wrapping AWS S3 calls with retry + circuit breaker | Infrastructure.AWS |
| **HATEOAS** | Hypermedia links on API responses | API |

---

## 🔐 Security Features

- JWT token-based authentication (configurable secret, issuer, audience)
- Role-based authorization (AdminOnly, ManagerOnly policies)
- ASP.NET Core Identity for user and role management
- Input validation via FluentValidation (pipeline + controller layer)
- SQL injection prevention (parameterized EF Core queries)
- Rate limiting: sliding window (global), fixed window (writes), strict fixed window (auth)
- CORS: configurable per-policy allowed origins
- HTTPS enforcement (Development environment)

---

## 📈 Scalability Considerations

**Current Implementation:**
- Stateless API design (horizontal scaling ready)
- Distributed caching with Garnet/Redis
- Database connection pooling via EF Core
- Background job processing with Quartz.NET
- .NET Aspire for local orchestration (cloud-deployable)
- Polly resilience policies on external S3 calls

**Future Enhancements:**
- Read replicas for reporting
- Event sourcing for complete audit trail
- Message queue integration (RabbitMQ/Azure Service Bus)
- Microservices decomposition (if needed)

---

## 🤖 AI Integration Ready

The system is designed to integrate with AI agents for:
- **Demand Forecasting**: Historical data export APIs
- **Reorder Optimization**: Stock level and supplier performance data
- **Anomaly Detection**: Transaction pattern analysis
- **Webhook Support**: Real-time stock level notifications

---

## 📚 Documentation

- [Architecture Overview](docs/architecture/smart-inventory-arch.md)
- [Domain Model](docs/architecture/smart-inventory-domain.md)
- [Database Schema](docs/architecture/smart-inventory-db-schema.md)
- API Documentation: Available at `/scalar/v1` when running

---

## 🛣️ Roadmap

### Phase 1 (Weeks 1-2) ✅
- Project setup and architecture
- Domain model implementation
- Database schema and EF Core migrations
- JWT authentication and ASP.NET Core Identity

### Phase 2 (Weeks 3-4) ✅
- Products and Warehouses CRUD with versioned controllers
- AWS S3 file upload (direct stream + pre-signed URLs)
- MediatR pipeline behaviors (Logging, Validation, UnitOfWork)
- Mapster type adapter mappings
- API versioning, rate limiting, CORS, HATEOAS links
- .NET Aspire orchestration

### Phase 3 (Weeks 5-6) 🔲
- Stock transaction management
- Purchase order workflow
- Alert system and notifications
- Full background job implementation
- Expanded test suite

---

## 🤝 Contributing

This is a portfolio project, but feedback and suggestions are welcome! Feel free to:
- Report issues
- Suggest improvements
- Share best practices

---

## 📄 License

This project is licensed under GPL-3.0 license - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Smart Inventory Management System Team**
*Jose Valdes*
Created as a portfolio project demonstrating modern .NET backend development practices.

---

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- .NET community for excellent libraries and patterns
- Microsoft for comprehensive documentation

---

## 📞 Support

For questions or issues:
- Check the [documentation](docs/)
- Review existing issues
- Create a new issue with detailed information

---

**Last Updated:** January 2026  
**Version:** 1.0.0  
**.NET Version:** 10.0
