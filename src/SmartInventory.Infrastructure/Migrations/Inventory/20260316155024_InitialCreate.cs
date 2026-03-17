using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations.Inventory
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.EnsureSchema(
                name: "Purchasing");

            migrationBuilder.EnsureSchema(
                name: "Alerts");

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasure = table.Column<int>(type: "int", nullable: false),
                    MinimumStockLevel = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    ReorderQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_MinimumStockLevel", "[MinimumStockLevel] >= 0");
                    table.CheckConstraint("CK_Products_ReorderPoint", "[ReorderPoint] >= 0");
                    table.CheckConstraint("CK_Products_ReorderQuantity", "[ReorderQuantity] > 0");
                    table.CheckConstraint("CK_Products_UnitCost", "[UnitCost] IS NULL OR [UnitCost] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinimumOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 3.0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.CheckConstraint("CK_Suppliers_Email", "[Email] IS NULL OR [Email] LIKE '%_@__%.__%'");
                    table.CheckConstraint("CK_Suppliers_LeadTimeDays", "[LeadTimeDays] >= 0");
                    table.CheckConstraint("CK_Suppliers_MinimumOrderValue", "[MinimumOrderValue] IS NULL OR [MinimumOrderValue] >= 0");
                    table.CheckConstraint("CK_Suppliers_Rating", "[Rating] >= 1.0 AND [Rating] <= 5.0");
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WarehouseType = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManagerEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManagerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.CheckConstraint("CK_Warehouses_Capacity", "[Capacity] IS NULL OR [Capacity] > 0");
                    table.CheckConstraint("CK_Warehouses_Email", "[ManagerEmail] IS NULL OR [ManagerEmail] LIKE '%_@__%.__%'");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.CheckConstraint("CK_PurchaseOrders_ExpectedDate", "[ExpectedDeliveryDate] >= [OrderDate]");
                    table.CheckConstraint("CK_PurchaseOrders_ShippingCost", "[ShippingCost] >= 0");
                    table.CheckConstraint("CK_PurchaseOrders_SubTotal", "[SubTotal] >= 0");
                    table.CheckConstraint("CK_PurchaseOrders_TaxAmount", "[TaxAmount] >= 0");
                    table.CheckConstraint("CK_PurchaseOrders_TotalAmount", "[TotalAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers",
                        column: x => x.SupplierId,
                        principalSchema: "Purchasing",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Warehouses",
                        column: x => x.WarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    QuantityReserved = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    QuantityAvailable = table.Column<decimal>(type: "decimal(18,4)", nullable: false, computedColumnSql: "[QuantityOnHand] - [QuantityReserved]", stored: true),
                    LastStockTakeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LastTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.Id);
                    table.CheckConstraint("CK_Stock_QuantityOnHand", "[QuantityOnHand] >= 0");
                    table.CheckConstraint("CK_Stock_QuantityReserved", "[QuantityReserved] >= 0");
                    table.CheckConstraint("CK_Stock_Reserved_LTE_OnHand", "[QuantityReserved] <= [QuantityOnHand]");
                    table.ForeignKey(
                        name: "FK_Stock_Products",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Warehouses",
                        column: x => x.WarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockAlerts",
                schema: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertType = table.Column<int>(type: "int", nullable: false),
                    CurrentQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ThresholdQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAlerts_Products",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAlerts_Warehouses",
                        column: x => x.WarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TransactionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReversedByTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.CheckConstraint("CK_StockTransactions_Quantity", "[Quantity] <> 0");
                    table.CheckConstraint("CK_StockTransactions_TransactionDate", "[TransactionDate] <= SYSUTCDATETIME()");
                    table.CheckConstraint("CK_StockTransactions_UnitCost", "[UnitCost] IS NULL OR [UnitCost] >= 0");
                    table.ForeignKey(
                        name: "FK_StockTransactions_DestinationWarehouse",
                        column: x => x.DestinationWarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_ReversedBy",
                        column: x => x.ReversedByTransactionId,
                        principalSchema: "Inventory",
                        principalTable: "StockTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_SourceWarehouse",
                        column: x => x.SourceWarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Warehouses",
                        column: x => x.WarehouseId,
                        principalSchema: "Inventory",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.CheckConstraint("CK_PurchaseOrderItems_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_PurchaseOrderItems_ReceivedQuantity", "[ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [Quantity]");
                    table.CheckConstraint("CK_PurchaseOrderItems_UnitCost", "[UnitCost] >= 0");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products",
                        column: x => x.ProductId,
                        principalSchema: "Inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                schema: "Inventory",
                table: "Products",
                column: "Category")
                .Annotation("SqlServer:Include", new[] { "Name", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                schema: "Inventory",
                table: "Products",
                column: "IsActive")
                .Annotation("SqlServer:Include", new[] { "SKU", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "Inventory",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "UK_Products_SKU",
                schema: "Inventory",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                column: "ProductId")
                .Annotation("SqlServer:Include", new[] { "PurchaseOrderId", "Quantity", "UnitCost" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId")
                .Annotation("SqlServer:Include", new[] { "ProductId", "Quantity", "UnitCost" });

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrderItems_PO_Product",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OrderDate",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "OrderDate",
                descending: new bool[0])
                .Annotation("SqlServer:Include", new[] { "OrderNumber", "Status", "TotalAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Status",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "Status")
                .Annotation("SqlServer:Include", new[] { "OrderNumber", "OrderDate", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "SupplierId")
                .Annotation("SqlServer:Include", new[] { "OrderDate", "Status", "TotalAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "WarehouseId")
                .Annotation("SqlServer:Include", new[] { "OrderDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrders_Number",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ProductId",
                schema: "Inventory",
                table: "Stock",
                column: "ProductId")
                .Annotation("SqlServer:Include", new[] { "WarehouseId", "QuantityOnHand", "QuantityAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_Stock_WarehouseId",
                schema: "Inventory",
                table: "Stock",
                column: "WarehouseId")
                .Annotation("SqlServer:Include", new[] { "ProductId", "QuantityOnHand", "QuantityAvailable" });

            migrationBuilder.CreateIndex(
                name: "UK_Stock_Product_Warehouse",
                schema: "Inventory",
                table: "Stock",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_CreatedAt",
                schema: "Alerts",
                table: "StockAlerts",
                column: "CreatedAt",
                descending: new bool[0])
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_ProductId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "ProductId")
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_Severity",
                schema: "Alerts",
                table: "StockAlerts",
                column: "Severity")
                .Annotation("SqlServer:Include", new[] { "Status", "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_Status",
                schema: "Alerts",
                table: "StockAlerts",
                column: "Status")
                .Annotation("SqlServer:Include", new[] { "ProductId", "WarehouseId", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "WarehouseId")
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Date",
                schema: "Inventory",
                table: "StockTransactions",
                column: "TransactionDate")
                .Annotation("SqlServer:Include", new[] { "ProductId", "WarehouseId", "Quantity", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_DestinationWarehouseId",
                schema: "Inventory",
                table: "StockTransactions",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                schema: "Inventory",
                table: "StockTransactions",
                column: "ProductId")
                .Annotation("SqlServer:Include", new[] { "TransactionDate", "Quantity", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Reference",
                schema: "Inventory",
                table: "StockTransactions",
                column: "ReferenceNumber",
                filter: "[ReferenceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ReversedByTransactionId",
                schema: "Inventory",
                table: "StockTransactions",
                column: "ReversedByTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_SourceWarehouseId",
                schema: "Inventory",
                table: "StockTransactions",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Type",
                schema: "Inventory",
                table: "StockTransactions",
                column: "TransactionType")
                .Annotation("SqlServer:Include", new[] { "ProductId", "WarehouseId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_WarehouseId",
                schema: "Inventory",
                table: "StockTransactions",
                column: "WarehouseId")
                .Annotation("SqlServer:Include", new[] { "TransactionDate", "ProductId", "Quantity" });

            migrationBuilder.CreateIndex(
                name: "UK_StockTransactions_Number",
                schema: "Inventory",
                table: "StockTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsActive",
                schema: "Purchasing",
                table: "Suppliers",
                column: "IsActive")
                .Annotation("SqlServer:Include", new[] { "Code", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Rating",
                schema: "Purchasing",
                table: "Suppliers",
                column: "Rating",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "UK_Suppliers_Code",
                schema: "Purchasing",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_IsActive",
                schema: "Inventory",
                table: "Warehouses",
                column: "IsActive")
                .Annotation("SqlServer:Include", new[] { "Code", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Type",
                schema: "Inventory",
                table: "Warehouses",
                column: "WarehouseType");

            migrationBuilder.CreateIndex(
                name: "UK_Warehouses_Code",
                schema: "Inventory",
                table: "Warehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderItems",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Stock",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockAlerts",
                schema: "Alerts");

            migrationBuilder.DropTable(
                name: "StockTransactions",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Warehouses",
                schema: "Inventory");
        }
    }
}
