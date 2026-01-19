
--Master product catalog
CREATE TABLE [Inventory].[Products] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [SKU] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [Category] INT NOT NULL, -- ProductCategory enum
    [UnitOfMeasure] INT NOT NULL, -- UnitOfMeasure enum
    [MinimumStockLevel] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [ReorderPoint] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [ReorderQuantity] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [UnitCost] DECIMAL(18, 4) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [UpdatedBy] NVARCHAR(100) NOT NULL,
    
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_Products_SKU] UNIQUE NONCLUSTERED ([SKU]),
    CONSTRAINT [CK_Products_MinimumStockLevel] CHECK ([MinimumStockLevel] >= 0),
    CONSTRAINT [CK_Products_ReorderPoint] CHECK ([ReorderPoint] >= 0),
    CONSTRAINT [CK_Products_ReorderQuantity] CHECK ([ReorderQuantity] > 0),
    CONSTRAINT [CK_Products_UnitCost] CHECK ([UnitCost] IS NULL OR [UnitCost] >= 0)
);
GO

--Inventory.Warehouses
CREATE TABLE [Inventory].[Warehouses] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Code] NVARCHAR(20) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [WarehouseType] INT NOT NULL, -- WarehouseType enum
    [Capacity] DECIMAL(18, 4) NULL,
    [ManagerName] NVARCHAR(100) NULL,
    [ManagerEmail] NVARCHAR(100) NULL,
    [ManagerPhone] NVARCHAR(20) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    
    -- Address (embedded value object)
    [Address_Street] NVARCHAR(200) NOT NULL,
    [Address_City] NVARCHAR(100) NOT NULL,
    [Address_State] NVARCHAR(100) NULL,
    [Address_PostalCode] NVARCHAR(20) NULL,
    [Address_Country] NVARCHAR(100) NOT NULL,
    
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT [PK_Warehouses] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_Warehouses_Code] UNIQUE NONCLUSTERED ([Code]),
    CONSTRAINT [CK_Warehouses_Capacity] CHECK ([Capacity] IS NULL OR [Capacity] > 0),
    CONSTRAINT [CK_Warehouses_Email] CHECK ([ManagerEmail] IS NULL OR [ManagerEmail] LIKE '%_@__%.__%')
);
GO

--Inventory.Stock
CREATE TABLE [Inventory].[Stock] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [QuantityOnHand] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [QuantityReserved] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [LastStockTakeDate] DATETIME2(7) NULL,
    [LastUpdatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [LastTransactionId] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_Stock] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_Stock_Product_Warehouse] UNIQUE NONCLUSTERED ([ProductId], [WarehouseId]),
    CONSTRAINT [FK_Stock_Products] FOREIGN KEY ([ProductId]) 
        REFERENCES [Inventory].[Products]([Id]),
    CONSTRAINT [FK_Stock_Warehouses] FOREIGN KEY ([WarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id]),
    CONSTRAINT [CK_Stock_QuantityOnHand] CHECK ([QuantityOnHand] >= 0),
    CONSTRAINT [CK_Stock_QuantityReserved] CHECK ([QuantityReserved] >= 0),
    CONSTRAINT [CK_Stock_Reserved_LTE_OnHand] CHECK ([QuantityReserved] <= [QuantityOnHand])
);
GO

-- Computed column for available quantity
ALTER TABLE [Inventory].[Stock] 
ADD [QuantityAvailable] AS ([QuantityOnHand] - [QuantityReserved]) PERSISTED;
GO


--Inventory.StockTransactions
CREATE TABLE [Inventory].[StockTransactions] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [TransactionNumber] NVARCHAR(50) NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionType] INT NOT NULL, -- TransactionType enum
    [Quantity] DECIMAL(18, 4) NOT NULL,
    [UnitCost] DECIMAL(18, 4) NULL,
    [TotalCost] DECIMAL(18, 4) NULL,
    [ReferenceNumber] NVARCHAR(100) NULL,
    [SourceWarehouseId] UNIQUEIDENTIFIER NULL, -- For transfers
    [DestinationWarehouseId] UNIQUEIDENTIFIER NULL, -- For transfers
    [Reason] NVARCHAR(500) NULL,
    [Notes] NVARCHAR(1000) NULL,
    [TransactionDate] DATETIME2(7) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [IsReversed] BIT NOT NULL DEFAULT 0,
    [ReversedByTransactionId] UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT [PK_StockTransactions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_StockTransactions_Number] UNIQUE NONCLUSTERED ([TransactionNumber]),
    CONSTRAINT [FK_StockTransactions_Products] FOREIGN KEY ([ProductId]) 
        REFERENCES [Inventory].[Products]([Id]),
    CONSTRAINT [FK_StockTransactions_Warehouses] FOREIGN KEY ([WarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id]),
    CONSTRAINT [FK_StockTransactions_SourceWarehouse] FOREIGN KEY ([SourceWarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id]),
    CONSTRAINT [FK_StockTransactions_DestinationWarehouse] FOREIGN KEY ([DestinationWarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id]),
    CONSTRAINT [FK_StockTransactions_ReversedBy] FOREIGN KEY ([ReversedByTransactionId]) 
        REFERENCES [Inventory].[StockTransactions]([Id]),
    CONSTRAINT [CK_StockTransactions_Quantity] CHECK ([Quantity] <> 0),
    CONSTRAINT [CK_StockTransactions_UnitCost] CHECK ([UnitCost] IS NULL OR [UnitCost] >= 0),
    CONSTRAINT [CK_StockTransactions_TransactionDate] CHECK ([TransactionDate] <= SYSUTCDATETIME())
);
GO


--5. Purchasing.Suppliers

CREATE TABLE [Purchasing].[Suppliers] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Code] NVARCHAR(20) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [ContactPerson] NVARCHAR(100) NULL,
    [Email] NVARCHAR(100) NULL,
    [Phone] NVARCHAR(20) NULL,
    [PaymentTerms] NVARCHAR(200) NULL,
    [LeadTimeDays] INT NOT NULL DEFAULT 0,
    [MinimumOrderValue] DECIMAL(18, 2) NULL,
    [Rating] DECIMAL(3, 2) NOT NULL DEFAULT 3.0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    
    -- Address (embedded value object)
    [Address_Street] NVARCHAR(200) NOT NULL,
    [Address_City] NVARCHAR(100) NOT NULL,
    [Address_State] NVARCHAR(100) NULL,
    [Address_PostalCode] NVARCHAR(20) NULL,
    [Address_Country] NVARCHAR(100) NOT NULL,
    
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_Suppliers_Code] UNIQUE NONCLUSTERED ([Code]),
    CONSTRAINT [CK_Suppliers_Email] CHECK ([Email] IS NULL OR [Email] LIKE '%_@__%.__%'),
    CONSTRAINT [CK_Suppliers_LeadTimeDays] CHECK ([LeadTimeDays] >= 0),
    CONSTRAINT [CK_Suppliers_Rating] CHECK ([Rating] >= 1.0 AND [Rating] <= 5.0),
    CONSTRAINT [CK_Suppliers_MinimumOrderValue] CHECK ([MinimumOrderValue] IS NULL OR [MinimumOrderValue] >= 0)
);
GO


-- 6. Purchasing.PurchaseOrders

CREATE TABLE [Purchasing].[PurchaseOrders] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OrderNumber] NVARCHAR(50) NOT NULL,
    [SupplierId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [OrderDate] DATETIME2(7) NOT NULL,
    [ExpectedDeliveryDate] DATETIME2(7) NOT NULL,
    [ActualDeliveryDate] DATETIME2(7) NULL,
    [Status] INT NOT NULL, -- PurchaseOrderStatus enum
    [SubTotal] DECIMAL(18, 2) NOT NULL DEFAULT 0,
    [TaxAmount] DECIMAL(18, 2) NOT NULL DEFAULT 0,
    [ShippingCost] DECIMAL(18, 2) NOT NULL DEFAULT 0,
    [TotalAmount] DECIMAL(18, 2) NOT NULL DEFAULT 0,
    [Notes] NVARCHAR(1000) NULL,
    [ApprovedBy] NVARCHAR(100) NULL,
    [ApprovedAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedBy] NVARCHAR(100) NOT NULL,
    
    CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_PurchaseOrders_Number] UNIQUE NONCLUSTERED ([OrderNumber]),
    CONSTRAINT [FK_PurchaseOrders_Suppliers] FOREIGN KEY ([SupplierId]) 
        REFERENCES [Purchasing].[Suppliers]([Id]),
    CONSTRAINT [FK_PurchaseOrders_Warehouses] FOREIGN KEY ([WarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id]),
    CONSTRAINT [CK_PurchaseOrders_ExpectedDate] CHECK ([ExpectedDeliveryDate] >= [OrderDate]),
    CONSTRAINT [CK_PurchaseOrders_SubTotal] CHECK ([SubTotal] >= 0),
    CONSTRAINT [CK_PurchaseOrders_TaxAmount] CHECK ([TaxAmount] >= 0),
    CONSTRAINT [CK_PurchaseOrders_ShippingCost] CHECK ([ShippingCost] >= 0),
    CONSTRAINT [CK_PurchaseOrders_TotalAmount] CHECK ([TotalAmount] >= 0)
);
GO

--7. Purchasing.PurchaseOrderItems

CREATE TABLE [Purchasing].[PurchaseOrderItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [PurchaseOrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] DECIMAL(18, 4) NOT NULL,
    [UnitCost] DECIMAL(18, 4) NOT NULL,
    [TotalCost] DECIMAL(18, 4) NOT NULL,
    [ReceivedQuantity] DECIMAL(18, 4) NOT NULL DEFAULT 0,
    [Notes] NVARCHAR(500) NULL,
    
    CONSTRAINT [PK_PurchaseOrderItems] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_PurchaseOrderItems_PO_Product] UNIQUE NONCLUSTERED ([PurchaseOrderId], [ProductId]),
    CONSTRAINT [FK_PurchaseOrderItems_PurchaseOrders] FOREIGN KEY ([PurchaseOrderId]) 
        REFERENCES [Purchasing].[PurchaseOrders]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrderItems_Products] FOREIGN KEY ([ProductId]) 
        REFERENCES [Inventory].[Products]([Id]),
    CONSTRAINT [CK_PurchaseOrderItems_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_PurchaseOrderItems_UnitCost] CHECK ([UnitCost] >= 0),
    CONSTRAINT [CK_PurchaseOrderItems_ReceivedQuantity] CHECK ([ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [Quantity])
);
GO

--### 8. Alerts.StockAlerts
CREATE TABLE [Alerts].[StockAlerts] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [WarehouseId] UNIQUEIDENTIFIER NOT NULL,
    [AlertType] INT NOT NULL, -- StockAlertType enum
    [CurrentQuantity] DECIMAL(18, 4) NOT NULL,
    [ThresholdQuantity] DECIMAL(18, 4) NOT NULL,
    [Message] NVARCHAR(500) NOT NULL,
    [Severity] INT NOT NULL, -- AlertSeverity enum
    [Status] INT NOT NULL DEFAULT 1, -- AlertStatus enum (1 = New)
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [AcknowledgedAt] DATETIME2(7) NULL,
    [AcknowledgedBy] NVARCHAR(100) NULL,
    [ResolvedAt] DATETIME2(7) NULL,
    [ResolvedBy] NVARCHAR(100) NULL,
    [ResolutionNotes] NVARCHAR(500) NULL,
    
    CONSTRAINT [PK_StockAlerts] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_StockAlerts_Products] FOREIGN KEY ([ProductId]) 
        REFERENCES [Inventory].[Products]([Id]),
    CONSTRAINT [FK_StockAlerts_Warehouses] FOREIGN KEY ([WarehouseId]) 
        REFERENCES [Inventory].[Warehouses]([Id])
);
GO


--### Inventory.ProductCategories
CREATE TABLE [Inventory].[ProductCategories] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UK_ProductCategories_Name] UNIQUE NONCLUSTERED ([Name])
);
GO

-- Seed data
INSERT INTO [Inventory].[ProductCategories] ([Id], [Name], [Description])
VALUES 
    (1, 'Electronics', 'Electronic devices and components'),
    (2, 'Consumables', 'Items that are consumed or used up'),
    (3, 'Equipment', 'Machinery and equipment'),
    (4, 'Tools', 'Hand tools and power tools'),
    (5, 'Safety', 'Safety equipment and PPE'),
    (6, 'RawMaterials', 'Raw materials for production'),
    (7, 'FinishedGoods', 'Finished products ready for sale'),
    (8, 'Packaging', 'Packaging materials'),
    (99, 'Other', 'Miscellaneous items');
GO


-- Transaction Number Sequence
CREATE SEQUENCE [Inventory].[TransactionNumberSequence]
    AS BIGINT
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    NO MAXVALUE
    CYCLE
    CACHE 50;
GO

-- Purchase Order Number Sequence
CREATE SEQUENCE [Purchasing].[PurchaseOrderNumberSequence]
    AS BIGINT
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    NO MAXVALUE
    CYCLE
    CACHE 50;
GO



-- Trigger for Products
CREATE TRIGGER [Inventory].[TR_Products_UpdatedAt]
ON [Inventory].[Products]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET [UpdatedAt] = SYSUTCDATETIME()
    FROM [Inventory].[Products] p
    INNER JOIN inserted i ON p.[Id] = i.[Id];
END;
GO

-- Trigger for Warehouses
CREATE TRIGGER [Inventory].[TR_Warehouses_UpdatedAt]
ON [Inventory].[Warehouses]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE w
    SET [UpdatedAt] = SYSUTCDATETIME()
    FROM [Inventory].[Warehouses] w
    INNER JOIN inserted i ON w.[Id] = i.[Id];
END;
GO

-- Trigger for Suppliers
CREATE TRIGGER [Purchasing].[TR_Suppliers_UpdatedAt]
ON [Purchasing].[Suppliers]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s
    SET [UpdatedAt] = SYSUTCDATETIME()
    FROM [Purchasing].[Suppliers] s
    INNER JOIN inserted i ON s.[Id] = i.[Id];
END;
GO

-- Trigger for Purchase Orders
CREATE TRIGGER [Purchasing].[TR_PurchaseOrders_UpdatedAt]
ON [Purchasing].[PurchaseOrders]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE po
    SET [UpdatedAt] = SYSUTCDATETIME()
    FROM [Purchasing].[PurchaseOrders] po
    INNER JOIN inserted i ON po.[Id] = i.[Id];
END;
GO


/*
-- Views for Common Queries
*/

CREATE VIEW [Inventory].[vw_StockSummary]
AS
SELECT 
    s.[Id] AS StockId,
    p.[SKU],
    p.[Name] AS ProductName,
    p.[Category],
    p.[UnitOfMeasure],
    w.[Code] AS WarehouseCode,
    w.[Name] AS WarehouseName,
    s.[QuantityOnHand],
    s.[QuantityReserved],
    s.[QuantityAvailable],
    p.[ReorderPoint],
    p.[MinimumStockLevel],
    CASE 
        WHEN s.[QuantityOnHand] <= p.[MinimumStockLevel] THEN 'Critical'
        WHEN s.[QuantityOnHand] <= p.[ReorderPoint] THEN 'Low'
        ELSE 'Normal'
    END AS StockStatus,
    s.[LastStockTakeDate],
    s.[LastUpdatedAt]
FROM [Inventory].[Stock] s
INNER JOIN [Inventory].[Products] p ON s.[ProductId] = p.[Id]
INNER JOIN [Inventory].[Warehouses] w ON s.[WarehouseId] = w.[Id]
WHERE p.[IsActive] = 1 AND w.[IsActive] = 1;
GO

--### Low Stock Alert View
CREATE VIEW [Inventory].[vw_LowStockProducts]
AS
SELECT 
    p.[Id] AS ProductId,
    p.[SKU],
    p.[Name] AS ProductName,
    w.[Id] AS WarehouseId,
    w.[Code] AS WarehouseCode,
    w.[Name] AS WarehouseName,
    s.[QuantityOnHand],
    p.[ReorderPoint],
    p.[ReorderQuantity],
    (p.[ReorderPoint] - s.[QuantityOnHand]) AS QuantityDeficit,
    CASE 
        WHEN s.[QuantityOnHand] <= p.[MinimumStockLevel] THEN 4 -- Critical
        WHEN s.[QuantityOnHand] <= (p.[ReorderPoint] * 0.5) THEN 3 -- High
        WHEN s.[QuantityOnHand] <= p.[ReorderPoint] THEN 2 -- Medium
        ELSE 1 -- Low
    END AS SuggestedSeverity
FROM [Inventory].[Stock] s
INNER JOIN [Inventory].[Products] p ON s.[ProductId] = p.[Id]
INNER JOIN [Inventory].[Warehouses] w ON s.[WarehouseId] = w.[Id]
WHERE p.[IsActive] = 1 
    AND w.[IsActive] = 1
    AND s.[QuantityOnHand] <= p.[ReorderPoint];
GO


--### Purchase Order Summary View

CREATE VIEW [Purchasing].[vw_PurchaseOrderSummary]
AS
SELECT 
    po.[Id],
    po.[OrderNumber],
    po.[OrderDate],
    po.[ExpectedDeliveryDate],
    po.[ActualDeliveryDate],
    po.[Status],
    s.[Code] AS SupplierCode,
    s.[Name] AS SupplierName,
    w.[Code] AS WarehouseCode,
    w.[Name] AS WarehouseName,
    COUNT(poi.[Id]) AS ItemCount,
    po.[TotalAmount],
    po.[CreatedBy],
    po.[CreatedAt]
FROM [Purchasing].[PurchaseOrders] po
INNER JOIN [Purchasing].[Suppliers] s ON po.[SupplierId] = s.[Id]
INNER JOIN [Inventory].[Warehouses] w ON po.[WarehouseId] = w.[Id]
LEFT JOIN [Purchasing].[PurchaseOrderItems] poi ON po.[Id] = poi.[PurchaseOrderId]
GROUP BY 
    po.[Id], po.[OrderNumber], po.[OrderDate], po.[ExpectedDeliveryDate],
    po.[ActualDeliveryDate], po.[Status], s.[Code], s.[Name], 
    w.[Code], w.[Name], po.[TotalAmount], po.[CreatedBy], po.[CreatedAt];
GO

--## Stored Procedures

CREATE PROCEDURE [Inventory].[sp_GetStockByWarehouse]
    @WarehouseId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.[Id],
        s.[ProductId],
        p.[SKU],
        p.[Name] AS ProductName,
        s.[QuantityOnHand],
        s.[QuantityReserved],
        s.[QuantityAvailable],
        s.[LastUpdatedAt]
    FROM [Inventory].[Stock] s
    INNER JOIN [Inventory].[Products] p ON s.[ProductId] = p.[Id]
    WHERE s.[WarehouseId] = @WarehouseId
        AND p.[IsActive] = 1
    ORDER BY p.[Name];
END;
GO

--### sp_GetStockTransactionHistory
CREATE PROCEDURE [Inventory].[sp_GetStockTransactionHistory]
    @ProductId UNIQUEIDENTIFIER = NULL,
    @WarehouseId UNIQUEIDENTIFIER = NULL,
    @StartDate DATETIME2(7) = NULL,
    @EndDate DATETIME2(7) = NULL,
    @TransactionType INT = NULL
AS
BEGIN
SET NOCOUNT ON;
SELECT 
    st.[Id],
    st.[TransactionNumber],
    st.[TransactionDate],
    st.[TransactionType],
    p.[SKU],
    p.[Name] AS ProductName,
    w.[Code] AS WarehouseCode,
    st.[Quantity],
    st.[UnitCost],
    st.[TotalCost],
    st.[ReferenceNumber],
    st.[Reason],
    st.[CreatedBy]
FROM [Inventory].[StockTransactions] st
INNER JOIN [Inventory].[Products] p ON st.[ProductId] = p.[Id]
INNER JOIN [Inventory].[Warehouses] w ON st.[WarehouseId] = w.[Id]
WHERE (@ProductId IS NULL OR st.[ProductId] = @ProductId)
    AND (@WarehouseId IS NULL OR st.[WarehouseId] = @WarehouseId)
    AND (@StartDate IS NULL OR st.[TransactionDate] >= @StartDate)
    AND (@EndDate IS NULL OR st.[TransactionDate] <= @EndDate)
    AND (@TransactionType IS NULL OR st.[TransactionType] = @TransactionType)
    AND st.[IsReversed] = 0
ORDER BY st.[TransactionDate] DESC;
END;
GO











