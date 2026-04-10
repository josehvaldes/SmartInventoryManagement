# Smart Inventory Management System - 6-Week Development Roadmap

## Overview

This roadmap breaks down the 6-week development timeline into manageable tasks, organized by week and day. Each task includes estimated time, dependencies, and deliverables to ensure steady progress toward a production-ready portfolio POC.

**Project Duration:** 6 weeks (30 working days)  
**Daily Commitment:** ~4-6 hours  
**Total Estimated Hours:** ~120-180 hours  
**Target Completion:** End of Week 6

---

## Week 1: Foundation & Setup (Days 1-5)

**Goal:** Establish project infrastructure, domain model, and database foundation

### Day 1: Project Initialization
**Time:** 4-5 hours

- [X] Create solution structure with all projects
  - SmartInventory.API
  - SmartInventory.Application
  - SmartInventory.Domain
  - SmartInventory.Infrastructure
  - SmartInventory.Contracts
  - Test projects
- [X] Install core NuGet packages
  - EF Core 10, SQL Server provider
  - MediatR
  - FluentValidation
  - Mapster
  - Serilog packages
- [X] Set up `.gitignore` and initial Git repository
- [X] Configure `appsettings.json` and `appsettings.Development.json`
- [X] Create initial folder structure within each project

**Deliverables:**
- ✅ Compiling solution with all projects
- ✅ Git repository initialized
- ✅ Dependencies installed

---

### Day 2: Domain Entities - Part 1
**Time:** 5-6 hours

- [X] Create base entity class with common properties (Id, CreatedAt, etc.)
- [X] Implement core domain entities:
  - `Product` entity with properties and business methods
  - `Warehouse` entity with Address value object
  - `Stock` entity with validation logic
- [X] Create `Address` value object with equality implementation
- [X] Define domain enums:
  - `ProductCategory`
  - `UnitOfMeasure`
  - `WarehouseType`
- [X] Add XML documentation comments

**Deliverables:**
- ✅ Product, Warehouse, Stock entities completed
- ✅ Address value object implemented
- ✅ Domain enums defined

---

### Day 3: Domain Entities - Part 2
**Time:** 5-6 hours

- [X] Implement remaining domain entities:
  - `StockTransaction` with immutability patterns
  - `Supplier` entity
  - `PurchaseOrder` aggregate root
  - `PurchaseOrderItem` entity
  - `StockAlert` entity
- [X] Create domain-specific exceptions:
  - `InsufficientStockException`
  - `InvalidStockOperationException`
  - `ProductNotFoundException`
  - `DuplicateEntityException`
- [X] Define domain events:
  - `StockLevelChangedEvent`
  - `ProductReorderPointReachedEvent`

**Deliverables:**
- ✅ All domain entities implemented
- ✅ Domain exceptions created
- ✅ Domain events defined

---

### Day 4: Database Schema & EF Core Configuration - Part 1
**Time:** 5-6 hours

- [X] Create SQL Server database manually or via script
- [X] Create database schemas (Inventory, Purchasing, Alerts, Audit)
- [X] Create `SmartInventoryDbContext` class
- [X] Implement EF Core entity configurations (Fluent API):
  - `ProductConfiguration`
  - `WarehouseConfiguration`
  - `StockConfiguration`
  - `StockTransactionConfiguration`
- [X] Configure value object mapping (Address using `OwnsOne`)


**Deliverables:**
- ✅ DbContext created
- ✅ Entity configurations for core entities completed
- ✅ Database created

---

### Day 5: Database Schema & EF Core Configuration - Part 2
**Time:** 5-6 hours

- [X] Complete remaining entity configurations:
  - `SupplierConfiguration`
  - `PurchaseOrderConfiguration`
  - `PurchaseOrderItemConfiguration`
  - `StockAlertConfiguration`
- [X] Verify database schema matches design
- [X] Create seed data script for development:
  - Sample warehouses
  - Sample products
  - Sample suppliers
- [X] Test database connectivity and migrations

**Deliverables:**
- ✅ All entity configurations completed
- ✅ Initial migration applied
- ✅ Seed data loaded
- ✅ Database fully operational

---


## Week 2: Infrastructure & Application Foundation (Days 6-10)

**Goal:** Build repository layer, set up MediatR, and implement first features

### Day 6: CQRS Pattern Implementation
**Time:** 5-6 hours
- [X] Set up MediatR in Application layer
- [X] Create `IQuery`, `ICommand`, `ICommandHandler`, and `IQueryHandler` generic interfaces
- [X] Create specific commands and queries
   - GetProductQuery
   - GetProductByIdQuery
   - CreateProductCommand
- [X] Implement a basic API/Products/ [GET] request
- [X] Register Data base Contexts interfaces to the API


**Deliverables:**
- ✅ CQRS pattern fully implemented
- ✅ Concreate DBContext classes registered in DI
- ✅ Basic GET request running.
---

### Day 7: MediatR Setup & Tools Setup
- [X] Create versioning  (APIExplorer)
   - add /api/versions endppint
   - add OpenAPI documentation /openapi/v1.json 
- [X] Add Logging (Serilog)
- [X] Add Validation(Fluent Validation) to the API
- [X] Add Caching strategy
- [X] Create MediatR pipeline behaviors:
  - `ValidationBehavior` (FluentValidation integration)
  - `LoggingBehavior` (request/response logging)
- [X] Add JWT Authentication
- [X] Add basic Unit tests and mocking infrastructure
- [X] Configure Quartz.NET in Infrastructure
- [X] Schedule job to run hourly
- [X] Test job execution manually

### Day 7: Products Feature - Part 1
**Time:** 5-6 hours

- [X] Implement rate limiter  into the API
- [X] Implement Infrastructure health checks from the API

- [X] Implement Products feature:
  - `CreateProductCommand` and `CreateProductCommandHandler`
  - `CreateProductValidator` (FluentValidation)
  - `ProductDto` 
- [X] Create contracts:
  - `CreateProductRequest`
  - `ProductResponse`

---


### Day 8: Products Feature - Part 2 & API Endpoints
**Time:** 5-6 hours

- [X] Implement remaining Products commands/queries:
  - `UpdateProductCommand`
  - `DeleteProductCommand`
  - `GetProductByIdQuery`
  - `GetAllProductsQuery` (with pagination)
- [X] Create Minimal API endpoints in `ProductsEndpoint.cs`:
  - POST `/api/products` - Create
  - PUT `/api/products/{id}` - Update
  - DELETE `/api/products/{id}` - Delete
  - GET `/api/products/{id}` - Get by ID
  - GET `/api/products` - Get all (paginated)
- [X] Add endpoint filters for validation
- [X] Test endpoints manually with API client (Postman/Insomnia)

**Deliverables:**
- ✅ Products CRUD complete
- ✅ Products API endpoints working
- ✅ Manual testing successful

---


### Day 9: Warehouses Feature
**Time:** 5-6 hours

- [X] Implement Warehouses feature (similar to Products):
  - `CreateWarehouseCommand` with validation
  - `GetWarehouseByIdQuery`
  - `GetAllWarehousesQuery`
  - Validators, DTOs, AutoMapper profiles
- [X] Create Warehouses API endpoints
- [X] Test warehouse operations
- [ ] Add business rule: Cannot delete warehouse with existing stock

**Deliverables:**
- ✅ Warehouses CRUD complete
- ✅ Warehouses API endpoints working
- ✅ Business rules enforced


**Document Version:** 1.1  
**Last Updated:** April 2026  
**Next Review:** End of Week 3
