# Smart Inventory Management System - Domain Model

## Overview

This document defines all domain entities, value objects, enumerations, and their relationships for the Smart Inventory Management System. This serves as the foundation for database schema design and business logic implementation.

---

## Domain Model Principles

### Rich Domain Model
- Entities contain business logic and validation
- Behavior is encapsulated within entities
- Domain rules are enforced at the entity level
- Value objects ensure immutability where appropriate

### Domain-Driven Design Concepts
- **Entities:** Objects with identity that persist over time
- **Value Objects:** Immutable objects defined by their attributes
- **Aggregates:** Clusters of entities treated as a single unit
- **Domain Events:** Notifications of significant business occurrences

---

## Core Entities

### 1. Product (Aggregate Root)

**Description:** Represents an item that can be stocked in warehouses

**Properties:**
```csharp
- Id: Guid (Primary Key)
- SKU: string (Stock Keeping Unit - unique, max 50 chars)
- Name: string (required, max 200 chars)
- Description: string (optional, max 1000 chars)
- Category: ProductCategory (enum)
- UnitOfMeasure: UnitOfMeasure (enum)
- MinimumStockLevel: decimal (must be >= 0)
- ReorderPoint: decimal (must be >= 0)
- ReorderQuantity: decimal (must be > 0)
- UnitCost: decimal (optional, for reference)
- IsActive: bool (default: true)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)
- CreatedBy: string (username/user ID)
- UpdatedBy: string (username/user ID)
```

**Business Rules:**
- SKU must be unique across all products
- ReorderPoint should be >= MinimumStockLevel
- ReorderQuantity should make sense for the UnitOfMeasure
- Cannot be deleted if stock exists in any warehouse
- When deactivated (IsActive = false), should not appear in new transactions

**Domain Methods:**
```csharp
- UpdateReorderSettings(decimal reorderPoint, decimal reorderQuantity)
- Activate() / Deactivate()
- UpdatePricing(decimal unitCost)
- ValidateStockLevel(decimal currentStock) -> returns bool (is below reorder point)
```

**Related Entities:**
- Has many: Stock (one per warehouse)
- Has many: StockTransaction
- Has many: PurchaseOrderItem
- Has many: StockAlert

---

### 2. Warehouse (Aggregate Root)

**Description:** Physical or logical location where inventory is stored

**Properties:**
```csharp
- Id: Guid (Primary Key)
- Code: string (unique, max 20 chars, e.g., "WH-001", "WH-MAIN")
- Name: string (required, max 200 chars)
- Address: Address (value object)
- WarehouseType: WarehouseType (enum)
- Capacity: decimal? (optional, in cubic meters or square feet)
- ManagerName: string (optional, max 100 chars)
- ManagerEmail: string (optional, max 100 chars)
- ManagerPhone: string (optional, max 20 chars)
- IsActive: bool (default: true)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)
```

**Business Rules:**
- Code must be unique
- Cannot be deleted if stock exists
- When deactivated, cannot receive new stock transactions
- Address must be valid

**Domain Methods:**
```csharp
- UpdateContactInfo(string managerName, string email, string phone)
- Activate() / Deactivate()
- CanAcceptStock() -> bool
- GetTotalStockValue() -> decimal
```

**Related Entities:**
- Has many: Stock
- Has many: StockTransaction
- Has many: PurchaseOrder (as destination)

---

### 3. Stock

**Description:** Current inventory level of a product at a specific warehouse

**Properties:**
```csharp
- Id: Guid (Primary Key)
- ProductId: Guid (Foreign Key to Product)
- WarehouseId: Guid (Foreign Key to Warehouse)
- QuantityOnHand: decimal (must be >= 0)
- QuantityReserved: decimal (must be >= 0, for future orders/holds)
- QuantityAvailable: decimal (computed: QuantityOnHand - QuantityReserved)
- LastStockTakeDate: DateTime? (optional, last physical count date)
- LastUpdatedAt: DateTime (UTC)
- LastTransactionId: Guid? (reference to last transaction)
```

**Business Rules:**
- Unique constraint on (ProductId, WarehouseId) - one stock record per product per warehouse
- QuantityReserved cannot exceed QuantityOnHand
- QuantityOnHand cannot go negative
- Any change to quantities must be recorded via StockTransaction

**Domain Methods:**
```csharp
- AddStock(decimal quantity, Guid transactionId)
- RemoveStock(decimal quantity, Guid transactionId)
- AdjustStock(decimal newQuantity, string reason, Guid transactionId)
- ReserveStock(decimal quantity) -> bool (returns false if insufficient)
- ReleaseReservedStock(decimal quantity)
- IsLowStock(decimal reorderPoint) -> bool
- PerformStockTake(decimal actualQuantity, string reason) -> creates adjustment transaction
```

**Related Entities:**
- Belongs to: Product
- Belongs to: Warehouse
- Has many: StockTransaction (via ProductId + WarehouseId)

---

### 4. StockTransaction (Aggregate Root)

**Description:** Records all stock movements and changes

**Properties:**
```csharp
- Id: Guid (Primary Key)
- TransactionNumber: string (unique, auto-generated, e.g., "TXN-2026-001234")
- ProductId: Guid (Foreign Key to Product)
- WarehouseId: Guid (Foreign Key to Warehouse)
- TransactionType: TransactionType (enum)
- Quantity: decimal (can be positive or negative depending on type)
- UnitCost: decimal? (optional, cost per unit at transaction time)
- TotalCost: decimal (computed: Quantity * UnitCost)
- ReferenceNumber: string (optional, max 100 chars - PO number, invoice, etc.)
- SourceWarehouseId: Guid? (for transfers, optional)
- DestinationWarehouseId: Guid? (for transfers, optional)
- Reason: string (optional, max 500 chars)
- Notes: string (optional, max 1000 chars)
- TransactionDate: DateTime (UTC, when transaction occurred)
- CreatedAt: DateTime (UTC, when record was created)
- CreatedBy: string (username/user ID)
- IsReversed: bool (default: false)
- ReversedByTransactionId: Guid? (reference to reversal transaction)
```

**Business Rules:**
- TransactionNumber must be unique and auto-generated
- Receipt and Adjustment (positive) increase stock
- Issue and Adjustment (negative) decrease stock
- Transfer requires both source and destination warehouses
- Cannot modify existing transactions (immutable)
- Reversals create a new opposite transaction
- All transactions must update the related Stock entity

**Domain Methods:**
```csharp
- Reverse(string reason) -> creates reversal transaction
- ValidateQuantity() -> ensures quantity makes sense for type
- CalculateTotalCost() -> returns Quantity * UnitCost
```

**Related Entities:**
- Belongs to: Product
- Belongs to: Warehouse
- May belong to: PurchaseOrder (if Receipt from PO)
- References: Stock (updates stock levels)

---

### 5. Supplier (Aggregate Root)

**Description:** Vendors who supply products

**Properties:**
```csharp
- Id: Guid (Primary Key)
- Code: string (unique, max 20 chars, e.g., "SUP-001")
- Name: string (required, max 200 chars)
- ContactPerson: string (optional, max 100 chars)
- Email: string (optional, max 100 chars)
- Phone: string (optional, max 20 chars)
- Address: Address (value object)
- PaymentTerms: string (optional, max 200 chars, e.g., "Net 30")
- LeadTimeDays: int (default: 0, estimated delivery time)
- MinimumOrderValue: decimal? (optional)
- Rating: decimal (1.0 to 5.0, default: 3.0)
- IsActive: bool (default: true)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)
```

**Business Rules:**
- Code must be unique
- Email must be valid format if provided
- Rating must be between 1.0 and 5.0
- Cannot be deleted if associated with purchase orders
- LeadTimeDays must be >= 0

**Domain Methods:**
```csharp
- UpdateRating(decimal newRating)
- UpdateLeadTime(int days)
- Activate() / Deactivate()
- CanFulfillOrder(decimal orderValue) -> bool (checks minimum order value)
```

**Related Entities:**
- Has many: PurchaseOrder
- Has many: SupplierProduct (junction table for product-specific info)

---

### 6. PurchaseOrder (Aggregate Root)

**Description:** Order placed with a supplier for products

**Properties:**
```csharp
- Id: Guid (Primary Key)
- OrderNumber: string (unique, auto-generated, e.g., "PO-2026-001234")
- SupplierId: Guid (Foreign Key to Supplier)
- WarehouseId: Guid (Foreign Key to Warehouse - destination)
- OrderDate: DateTime (UTC)
- ExpectedDeliveryDate: DateTime (UTC)
- ActualDeliveryDate: DateTime? (optional, set when received)
- Status: PurchaseOrderStatus (enum)
- SubTotal: decimal (sum of all items)
- TaxAmount: decimal (default: 0)
- ShippingCost: decimal (default: 0)
- TotalAmount: decimal (computed: SubTotal + TaxAmount + ShippingCost)
- Notes: string (optional, max 1000 chars)
- ApprovedBy: string (optional, username/user ID)
- ApprovedAt: DateTime? (optional)
- CreatedAt: DateTime (UTC)
- CreatedBy: string (username/user ID)
- UpdatedAt: DateTime (UTC)
- UpdatedBy: string (username/user ID)
```

**Collections:**
```csharp
- Items: List<PurchaseOrderItem> (one-to-many, cascade delete)
```

**Business Rules:**
- OrderNumber must be unique and auto-generated
- Cannot modify items after status is "Received" or "Cancelled"
- ExpectedDeliveryDate must be >= OrderDate
- Status transitions: Draft -> Submitted -> Confirmed -> Received (or Cancelled)
- Cannot be deleted after submission (can only cancel)
- When received, must create StockTransaction records

**Domain Methods:**
```csharp
- AddItem(PurchaseOrderItem item)
- RemoveItem(Guid itemId)
- Submit() -> changes status to Submitted
- Confirm() -> changes status to Confirmed
- Receive(DateTime actualDate) -> creates stock transactions, changes status
- Cancel(string reason) -> changes status to Cancelled
- CalculateTotals() -> recalculates SubTotal and TotalAmount
```

**Related Entities:**
- Belongs to: Supplier
- Belongs to: Warehouse
- Has many: PurchaseOrderItem

---

### 7. PurchaseOrderItem

**Description:** Line item within a purchase order

**Properties:**
```csharp
- Id: Guid (Primary Key)
- PurchaseOrderId: Guid (Foreign Key to PurchaseOrder)
- ProductId: Guid (Foreign Key to Product)
- Quantity: decimal (must be > 0)
- UnitCost: decimal (must be >= 0)
- TotalCost: decimal (computed: Quantity * UnitCost)
- ReceivedQuantity: decimal (default: 0)
- Notes: string (optional, max 500 chars)
```

**Business Rules:**
- Cannot have duplicate products in same PO
- Quantity must be positive
- ReceivedQuantity cannot exceed Quantity
- UnitCost should be validated against current product cost (warning if significantly different)

**Domain Methods:**
```csharp
- ReceiveQuantity(decimal quantity) -> updates ReceivedQuantity
- IsFullyReceived() -> bool (ReceivedQuantity >= Quantity)
- CalculateTotalCost() -> Quantity * UnitCost
```

**Related Entities:**
- Belongs to: PurchaseOrder
- Belongs to: Product

---

### 8. StockAlert (Aggregate Root)

**Description:** Notifications for stock conditions requiring attention

**Properties:**
```csharp
- Id: Guid (Primary Key)
- ProductId: Guid (Foreign Key to Product)
- WarehouseId: Guid (Foreign Key to Warehouse)
- AlertType: StockAlertType (enum)
- CurrentQuantity: decimal
- ThresholdQuantity: decimal (the level that triggered the alert)
- Message: string (auto-generated description, max 500 chars)
- Severity: AlertSeverity (enum: Low, Medium, High, Critical)
- Status: AlertStatus (enum)
- CreatedAt: DateTime (UTC)
- AcknowledgedAt: DateTime? (optional)
- AcknowledgedBy: string (optional, username/user ID)
- ResolvedAt: DateTime? (optional)
- ResolvedBy: string (optional, username/user ID)
- ResolutionNotes: string (optional, max 500 chars)
```

**Business Rules:**
- Same product/warehouse can have multiple active alerts of different types
- Cannot create duplicate active alerts of same type for same product/warehouse
- Status transitions: New -> Acknowledged -> Resolved (or Ignored)
- Auto-resolve when stock levels return to normal

**Domain Methods:**
```csharp
- Acknowledge(string userId)
- Resolve(string userId, string notes)
- Ignore(string userId, string reason)
- ShouldAutoResolve(decimal currentStockLevel) -> bool
```

**Related Entities:**
- Belongs to: Product
- Belongs to: Warehouse

---

## Value Objects

### Address

**Description:** Represents a physical address (immutable)

**Properties:**
```csharp
- Street: string (required, max 200 chars)
- City: string (required, max 100 chars)
- State: string (optional, max 100 chars)
- PostalCode: string (optional, max 20 chars)
- Country: string (required, max 100 chars)
```

**Business Rules:**
- All required fields must have values
- Country should use ISO country codes (optional validation)
- Immutable - create new instance for changes

**Methods:**
```csharp
- GetFullAddress() -> string (formatted address)
- Equals(Address other) -> bool (value equality)
```

**Usage:**
- Warehouse.Address
- Supplier.Address

---

### Money (Optional - for future)

**Description:** Represents monetary value with currency (immutable)

**Properties:**
```csharp
- Amount: decimal
- Currency: string (ISO 4217 currency code, e.g., "USD", "EUR")
```

**Business Rules:**
- Amount can be negative (for credits/refunds)
- Currency must be valid ISO code
- Cannot perform operations on different currencies

**Methods:**
```csharp
- Add(Money other) -> Money
- Subtract(Money other) -> Money
- Multiply(decimal factor) -> Money
- Equals(Money other) -> bool
```

---

## Enumerations

### ProductCategory

```csharp
public enum ProductCategory
{
    Electronics = 1,
    Consumables = 2,
    Equipment = 3,
    Tools = 4,
    Safety = 5,
    RawMaterials = 6,
    FinishedGoods = 7,
    Packaging = 8,
    Other = 99
}
```

---

### UnitOfMeasure

```csharp
public enum UnitOfMeasure
{
    Piece = 1,      // Individual items
    Box = 2,        // Box/carton
    Pallet = 3,     // Pallet
    Kilogram = 10,  // Weight
    Gram = 11,
    Pound = 12,
    Liter = 20,     // Volume
    Milliliter = 21,
    Gallon = 22,
    Meter = 30,     // Length
    Centimeter = 31,
    Foot = 32,
    SquareMeter = 40, // Area
    CubicMeter = 50   // Volume (cubic)
}
```

---

### WarehouseType

```csharp
public enum WarehouseType
{
    Main = 1,           // Primary warehouse
    Regional = 2,       // Regional distribution center
    Transit = 3,        // Temporary/in-transit storage
    ReturnCenter = 4,   // For returns and refurbishment
    Virtual = 5         // Logical warehouse (no physical location)
}
```

---

### TransactionType

```csharp
public enum TransactionType
{
    Receipt = 1,        // Receiving stock (increases quantity)
    Issue = 2,          // Issuing/selling stock (decreases quantity)
    Adjustment = 3,     // Manual adjustment (can be +/-)
    Transfer = 4,       // Transfer between warehouses
    Return = 5,         // Return to supplier (decreases quantity)
    Damage = 6,         // Damaged goods write-off (decreases quantity)
    StockTake = 7       // Physical count adjustment
}
```

---

### PurchaseOrderStatus

```csharp
public enum PurchaseOrderStatus
{
    Draft = 1,          // Being created
    Submitted = 2,      // Sent to supplier
    Confirmed = 3,      // Supplier confirmed
    PartiallyReceived = 4, // Some items received
    Received = 5,       // Fully received
    Cancelled = 6,      // Order cancelled
    Closed = 7          // Administratively closed
}
```

---

### StockAlertType

```csharp
public enum StockAlertType
{
    LowStock = 1,           // Below minimum stock level
    BelowReorderPoint = 2,  // Below reorder point
    Overstock = 3,          // Excessive stock (optional)
    NoMovement = 4,         // No transactions in X days (optional)
    NegativeStock = 5       // Data integrity issue (should never happen)
}
```

---

### AlertStatus

```csharp
public enum AlertStatus
{
    New = 1,            // Just created
    Acknowledged = 2,   // Seen by user
    InProgress = 3,     // Being addressed
    Resolved = 4,       // Issue resolved
    Ignored = 5         // Marked as non-issue
}
```

---

### AlertSeverity

```csharp
public enum AlertSeverity
{
    Low = 1,        // FYI only
    Medium = 2,     // Should address soon
    High = 3,       // Address today
    Critical = 4    // Immediate action required
}
```

---

## Domain Events

Domain events notify other parts of the system when significant business events occur.

### StockLevelChangedEvent

```csharp
- ProductId: Guid
- WarehouseId: Guid
- OldQuantity: decimal
- NewQuantity: decimal
- ChangeReason: string
- OccurredAt: DateTime
```

**Subscribers:**
- Alert system (to check for low stock)
- Analytics service (for demand forecasting)
- External integrations (webhooks)

---

### ProductReorderPointReachedEvent

```csharp
- ProductId: Guid
- WarehouseId: Guid
- CurrentQuantity: decimal
- ReorderPoint: decimal
- ReorderQuantity: decimal
- OccurredAt: DateTime
```

**Subscribers:**
- Alert system (creates StockAlert)
- Purchasing system (suggests PurchaseOrder)
- Notification service (emails procurement team)

---

### PurchaseOrderReceivedEvent

```csharp
- PurchaseOrderId: Guid
- OrderNumber: string
- SupplierId: Guid
- WarehouseId: Guid
- TotalItems: int
- TotalValue: decimal
- ReceivedAt: DateTime
```

**Subscribers:**
- Accounting system
- Inventory reports
- Supplier performance tracking

---

### StockTransactionCreatedEvent

```csharp
- TransactionId: Guid
- TransactionType: TransactionType
- ProductId: Guid
- WarehouseId: Guid
- Quantity: decimal
- CreatedAt: DateTime
```

**Subscribers:**
- Audit log
- Analytics
- Real-time dashboards

---

## Domain Exceptions

Custom exceptions for domain-specific errors:

### InsufficientStockException

```csharp
- ProductId: Guid
- WarehouseId: Guid
- RequestedQuantity: decimal
- AvailableQuantity: decimal
- Message: "Insufficient stock for product {ProductId} at warehouse {WarehouseId}"
```

---

### InvalidStockOperationException

```csharp
- Reason: string
- Message: Custom based on reason
```

**Examples:**
- Cannot issue stock from inactive warehouse
- Cannot transfer to same warehouse
- Negative quantity not allowed

---

### ProductNotFoundException

```csharp
- ProductId: Guid
- Message: "Product {ProductId} not found"
```

---

### DuplicateEntityException

```csharp
- EntityType: string
- ConflictingValue: string
- Message: "{EntityType} with {ConflictingValue} already exists"
```

**Examples:**
- Duplicate SKU
- Duplicate warehouse code

---

## Aggregate Roots and Boundaries

### Product Aggregate
- **Root:** Product
- **Contains:** None (stands alone)
- **References:** Stock, StockTransaction (external aggregates)

### Warehouse Aggregate
- **Root:** Warehouse
- **Contains:** None (stands alone)
- **References:** Stock, StockTransaction (external aggregates)

### Stock Aggregate
- **Root:** Stock
- **Contains:** None
- **Bounded Context:** Must be accessed through repository, not navigational property

### StockTransaction Aggregate
- **Root:** StockTransaction
- **Contains:** None (immutable record)
- **Creates:** Updates Stock aggregate via domain service

### Supplier Aggregate
- **Root:** Supplier
- **Contains:** None
- **References:** PurchaseOrder (external aggregate)

### PurchaseOrder Aggregate
- **Root:** PurchaseOrder
- **Contains:** PurchaseOrderItem (child entities)
- **Lifecycle:** Items cannot exist without parent PurchaseOrder

### StockAlert Aggregate
- **Root:** StockAlert
- **Contains:** None
- **Created By:** Background jobs and domain events

---

## Entity Relationships Summary

```
Product (1) ──────< (*) Stock
Product (1) ──────< (*) StockTransaction
Product (1) ──────< (*) PurchaseOrderItem
Product (1) ──────< (*) StockAlert

Warehouse (1) ────< (*) Stock
Warehouse (1) ────< (*) StockTransaction
Warehouse (1) ────< (*) PurchaseOrder
Warehouse (1) ────< (*) StockAlert

Supplier (1) ─────< (*) PurchaseOrder

PurchaseOrder (1) < (*) PurchaseOrderItem

Stock (1) ────────< (*) StockTransaction (logical relationship)
```

---

## Invariants and Business Rules

### Global Invariants

1. **Stock Consistency:** Stock.QuantityOnHand must always equal the sum of all StockTransactions for that Product/Warehouse combination
2. **No Negative Stock:** Stock.QuantityOnHand >= 0 at all times
3. **Reserved Stock:** Stock.QuantityReserved <= Stock.QuantityOnHand
4. **Immutable Transactions:** StockTransaction records cannot be modified, only reversed
5. **Active Entity Rules:** Inactive products/warehouses cannot participate in new transactions

### Consistency Guarantees

- All stock changes must create a corresponding StockTransaction
- Purchase order receipt must update stock and create transactions atomically
- Transfers must create two transactions (issue from source, receipt at destination)

---

## Domain Services

Domain services handle operations that don't naturally fit into a single entity.

### StockTransferService

**Purpose:** Coordinate transfers between warehouses

**Method:**
```csharp
TransferStock(
    Guid productId,
    Guid sourceWarehouseId,
    Guid destinationWarehouseId,
    decimal quantity,
    string reason
) -> (StockTransaction issueTransaction, StockTransaction receiptTransaction)
```

**Responsibilities:**
- Validate sufficient stock at source
- Create issue transaction at source
- Create receipt transaction at destination
- Update both Stock records
- Publish domain events

---

### StockValuationService

**Purpose:** Calculate stock value using different methods

**Methods:**
```csharp
CalculateFIFO(Guid productId, Guid warehouseId) -> decimal
CalculateLIFO(Guid productId, Guid warehouseId) -> decimal
CalculateWeightedAverage(Guid productId, Guid warehouseId) -> decimal
```

---

### LowStockCheckService

**Purpose:** Identify products below reorder points

**Method:**
```csharp
CheckLowStock() -> List<StockAlert>
```

**Responsibilities:**
- Query all stock records
- Compare against product reorder points
- Create StockAlert entities for violations
- Publish domain events

---

## Validation Rules Summary

### Product
- ✓ SKU: Required, unique, max 50 chars
- ✓ Name: Required, max 200 chars
- ✓ ReorderPoint >= 0
- ✓ ReorderQuantity > 0
- ✓ MinimumStockLevel >= 0

### Warehouse
- ✓ Code: Required, unique, max 20 chars
- ✓ Name: Required, max 200 chars
- ✓ Address: Must be valid
- ✓ Capacity: >= 0 if provided

### Stock
- ✓ Unique (ProductId, WarehouseId)
- ✓ QuantityOnHand >= 0
- ✓ QuantityReserved >= 0
- ✓ QuantityReserved <= QuantityOnHand

### StockTransaction
- ✓ TransactionNumber: Unique, auto-generated
- ✓ Quantity: Must be appropriate for type
- ✓ TransactionDate: Cannot be future date
- ✓ Transfer: Must have both source and destination

### Supplier
- ✓ Code: Required, unique, max 20 chars
- ✓ Email: Valid format if provided
- ✓ Rating: Between 1.0 and 5.0
- ✓ LeadTimeDays >= 0

### PurchaseOrder
- ✓ OrderNumber: Unique, auto-generated
- ✓ ExpectedDeliveryDate >= OrderDate
- ✓ Must have at least one item
- ✓ Status transitions must be valid

---

## Next Steps

This domain model will be translated into:

1. **C# Domain Entities** - Classes with properties and methods
2. **EF Core Configurations** - Fluent API configurations for database mapping
3. **Database Schema** - SQL Server tables, relationships, constraints
4. **Repository Interfaces** - Data access contracts
5. **Domain Events** - Event classes and handlers

---

**Document Version:** 1.0  
**Last Updated:** January 2026  
**Related Documents:** Architecture Overview, Database Schema Design