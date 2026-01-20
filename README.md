# Smart Inventory Management System - Cinnamon AI Agents

A modern, enterprise-grade inventory management system built with .NET 10, demonstrating 'Pragmatic' Clean Architecture principles, Domain-Driven Design, and modern backend development practices.

## 🎯 Project Overview

**Purpose:** Portfolio POC showcasing production-ready .NET development with AI integration capabilities  
**Timeline:** 6-week development cycle  
**Architecture:** 'Pargmatic' Clean Architecture with Vertical Slice flexibility

This system manages product inventory across multiple warehouses, handles purchase orders, tracks stock movements, and provides automated alerts for low stock conditions.

---

## 🏗️ Architecture

### Design Approach

The project implements a **hybrid architecture** combining:
- **Pragmatic Clean Architecture (DbContext abstraction)** for clear separation of concerns and testability
- **Vertical Slice Architecture** for feature-focused development
- **Domain-Driven Design (DDD)** with rich domain models
- **CQRS-lite** for separating read and write operations
- ** Specification Pattern ** for reusable and complex EF queries.

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
- **ASP.NET Core Minimal APIs** - Lightweight, high-performance APIs

### Database & Data Access
- **SQL Server 2025** - Primary database
- **Entity Framework Core 10** - ORM for standard operations
- **Dapper** - High-performance queries for complex reports

### Caching & Background Processing
- **StackExchange.Redis** - Distributed caching
- **Quartz.NET** - Scheduled jobs (low stock checks, reorder suggestions)

### Validation & Mapping
- **FluentValidation 11.x** - Request validation
- **AutoMapper 13.x** - DTO mapping

### Architecture Patterns
- **MediatR** - Command/Query handling (Mediator pattern)
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management

### Logging & Monitoring
- **Serilog** - Structured logging with SQL Server sink
- **Health Checks** - SQL Server, Redis, and application health monitoring

### API Documentation
- **Scalar** - Modern OpenAPI documentation UI

### Security
- **JWT Bearer Authentication** - Token-based auth
- **BCrypt.Net** - Password hashing

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Readable assertions
- **NSubstitute** - Mocking
- **Testcontainers** (optional) - Integration tests with real databases

---

## 📁 Solution Structure

```
SmartInventory/
│
├── src/
│   ├── SmartInventory.API/              # Presentation Layer
│   │   ├── Endpoints/                   # Minimal API endpoints
│   │   ├── Middleware/                  # Exception handling, logging
│   │   ├── Filters/                     # Validation filters
│   │   └── Program.cs                   # Entry point
│   │
│   ├── SmartInventory.Application/      # Application Layer
│   │   ├── Features/                    # Vertical slices by feature
│   │   │   ├── Products/
│   │   │   ├── Warehouses/
│   │   │   ├── Stock/
│   │   │   ├── Suppliers/
│   │   │   └── PurchaseOrders/
│   │   ├── Common/
│   │   │   ├── Behaviors/               # MediatR pipelines
│   │   │   ├── DTOs/
│   │   │   └── Validation/
│   │   └── Interfaces/
│   │
│   ├── SmartInventory.Domain/           # Domain Layer (Core)
│   │   ├── Entities/                    # Domain entities
│   │   ├── ValueObjects/                # Address, Money
│   │   ├── Enums/                       # Domain enumerations
│   │   ├── Events/                      # Domain events
│   │   └── Interfaces/                  # Repository interfaces
│   │
│   ├── SmartInventory.Infrastructure/   # Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── Context/                 # EF Core DbContext
│   │   │   ├── Configurations/          # Entity configurations
│   │   │   ├── Repositories/            # Repository implementations
│   │   │   └── Migrations/
│   │   ├── BackgroundJobs/              # Quartz.NET jobs
│   │   ├── Caching/                     # Redis implementation
│   │   └── Logging/                     # Serilog setup
│   │
│   └── SmartInventory.Contracts/        # Shared DTOs
│       ├── Requests/
│       └── Responses/
│
├── tests/
│   ├── SmartInventory.UnitTests/
│   ├── SmartInventory.IntegrationTests/
│   └── SmartInventory.ArchitectureTests/
│
└── docs/
    ├── architecture/                     # Architecture documents
    ├── api/                             # API documentation
    └── setup/                           # Setup guides
```

---

## 🎯 Core Features

### Inventory Management
- ✅ Product catalog with SKU management
- ✅ Multi-warehouse support
- ✅ Real-time stock tracking
- ✅ Stock reservations for orders
- ✅ Automated low stock alerts

### Stock Transactions
- ✅ Receipt, Issue, Transfer, Adjustment tracking
- ✅ Immutable audit trail
- ✅ Transaction reversal support
- ✅ Historical reporting

### Purchase Orders
- ✅ Supplier management
- ✅ PO creation and approval workflow
- ✅ Automated stock updates on receipt
- ✅ Status tracking (Draft → Submitted → Confirmed → Received)

### Alerts & Notifications
- ✅ Low stock alerts
- ✅ Reorder point monitoring
- ✅ Alert severity levels
- ✅ Acknowledgment and resolution workflow

### Background Jobs
- ✅ Automated low stock checks (hourly)
- ✅ Daily stock snapshots
- ✅ Reorder suggestions

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2025 Developer](https://www.microsoft.com/sql-server/sql-server-downloads)
- [Redis](https://redis.io/download) (optional for local development)
- IDE: Visual Studio 2026, VS Code

### Installation

1. **Clone the repository**
   ```powershell
   git clone https://github.com/josehvaldes/SmartInventoryManagement.git
   cd SmartInventoryManagement
   ```

2. **Update connection strings**
   
   Edit `src/SmartInventory.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=SmartInventoryDB;Trusted_Connection=True;TrustServerCertificate=True;",
       "RedisConnection": "localhost:6379"
     }
   }
   ```

3. **Restore NuGet packages**
   ```powershell
   dotnet restore
   ```

4. **Create database**
   ```powershell
   cd \scripts\database
   sqlcmd -S yourServerName[\instanceName] -i schema.sql 

   ```

5. **Run the application**
   ```powershell
   cd ../SmartInventory.API
   dotnet run
   ```

6. **Access API documentation**
   
   Open browser to: `https://localhost:5001/scalar/v1` (or configured port)

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

---

## 📊 Database Schema

The system uses SQL Server with the following schema organization:

- **Inventory Schema**: Products, Warehouses, Stock, StockTransactions
- **Purchasing Schema**: Suppliers, PurchaseOrders, PurchaseOrderItems
- **Alerts Schema**: StockAlerts
- **Audit Schema**: Audit logs (future)

See [Database Schema Documentation](docs/architecture/smart-inventory-db-schema.md) for detailed schema design.

---

## 🎨 Design Patterns

This project demonstrates the following design patterns:

| Pattern | Purpose | Location |
|---------|---------|----------|
| **Repository** | Data access abstraction | Infrastructure |
| **Unit of Work** | Transaction management | Infrastructure |
| **Mediator (MediatR)** | Decoupled request handling | Application |
| **CQRS** | Separate read/write operations | Application |
| **Strategy** | Inventory valuation algorithms | Domain/Application |
| **Factory** | Object creation | Infrastructure |
| **Specification** | Business rule encapsulation | Domain |

---

## 🔐 Security Features

- JWT token-based authentication
- Role-based authorization (RBAC)
- Secure password hashing (BCrypt)
- Input validation and sanitization
- SQL injection prevention (parameterized queries)
- HTTPS enforcement
- API rate limiting

---

## 📈 Scalability Considerations

**Current Implementation:**
- Stateless API design (horizontal scaling ready)
- Distributed caching with Redis
- Database connection pooling
- Background job processing

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
- Database schema creation

### Phase 2 (Weeks 3-4)
- Core CRUD operations
- Stock transaction management
- Purchase order workflow

### Phase 3 (Weeks 5-6)
- Alert system
- Background jobs
- API documentation
- Testing suite

### Future Enhancements
- Web dashboard (Blazor/React)
- Advanced reporting
- Multi-tenant support
- Barcode scanning integration

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
