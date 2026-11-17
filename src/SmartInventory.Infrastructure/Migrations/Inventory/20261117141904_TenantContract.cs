using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations.Inventory
{
    /// <inheritdoc />
    public partial class TenantContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_Warehouses_Code",
                schema: "Inventory",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "UK_Suppliers_Code",
                schema: "Purchasing",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "UK_StockTransactions_Number",
                schema: "Inventory",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_ProductId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "UK_Stock_Product_Warehouse",
                schema: "Inventory",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "UK_PurchaseOrders_Number",
                schema: "Purchasing",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "UK_PurchaseOrderItems_PO_Product",
                schema: "Purchasing",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "UK_Products_SKU",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "StockTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Alerts",
                table: "StockAlerts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Stock",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UK_Warehouses_Tenant_Code",
                schema: "Inventory",
                table: "Warehouses",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Suppliers_Tenant_Code",
                schema: "Purchasing",
                table: "Suppliers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_StockTransactions_Tenant_TransactionNumber",
                schema: "Inventory",
                table: "StockTransactions",
                columns: new[] { "TenantId", "TransactionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_ProductId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_Tenant_ProductId",
                schema: "Alerts",
                table: "StockAlerts",
                columns: new[] { "TenantId", "ProductId" })
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_Tenant_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts",
                columns: new[] { "TenantId", "WarehouseId" })
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "UK_Stock_Tenant_Product_Warehouse",
                schema: "Inventory",
                table: "Stock",
                columns: new[] { "TenantId", "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrders_Tenant_OrderNumber",
                schema: "Purchasing",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrderItems_Tenant_PO_Product",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                columns: new[] { "TenantId", "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Products_Tenant_SKU",
                schema: "Inventory",
                table: "Products",
                columns: new[] { "TenantId", "SKU" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tenants_TenantId",
                schema: "Inventory",
                table: "Products",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                 name: "FK_Stock_Tenants_TenantId",
                 schema: "Inventory",
                 table: "Stock",
                 column: "TenantId",
                 principalSchema: "Auth",
                 principalTable: "Tenants",
                 principalColumn: "Id",
                 onDelete: ReferentialAction.NoAction
            );

            migrationBuilder.AddForeignKey(
                 name: "FK_StockTransactions_Tenants_TenantId",
                 schema: "Inventory",
                 table: "StockTransactions",
                 column: "TenantId",
                 principalSchema: "Auth",
                 principalTable: "Tenants",
                 principalColumn: "Id",
                 onDelete: ReferentialAction.NoAction
             );


            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Tenants_TenantId",
                schema: "Inventory",
                table: "Warehouses",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );


            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Tenants_TenantId",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Tenants_TenantId",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );


            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Tenants_TenantId",
                schema: "Purchasing",
                table: "Suppliers",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );


            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_Tenants_TenantId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "TenantId",
                principalSchema: "Auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction
            );


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tenants_TenantId",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Tenants_TenantId",
                schema: "Inventory",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Tenants_TenantId",
                schema: "Inventory",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_Tenants_TenantId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Tenants_TenantId",
                schema: "Purchasing",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Tenants_TenantId",
                schema: "Purchasing",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Tenants_TenantId",
                schema: "Purchasing",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Stock_Tenants_TenantId",
                schema: "Inventory",
                table: "Stock");


            migrationBuilder.DropIndex(
                name: "UK_Warehouses_Tenant_Code",
                schema: "Inventory",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "UK_Suppliers_Tenant_Code",
                schema: "Purchasing",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "UK_StockTransactions_Tenant_TransactionNumber",
                schema: "Inventory",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_ProductId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_Tenant_ProductId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_Tenant_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_StockAlerts_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "UK_Stock_Tenant_Product_Warehouse",
                schema: "Inventory",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "UK_PurchaseOrders_Tenant_OrderNumber",
                schema: "Purchasing",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "UK_PurchaseOrderItems_Tenant_PO_Product",
                schema: "Purchasing",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "UK_Products_Tenant_SKU",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "StockTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Alerts",
                table: "StockAlerts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Stock",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "UK_Warehouses_Code",
                schema: "Inventory",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Suppliers_Code",
                schema: "Purchasing",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_StockTransactions_Number",
                schema: "Inventory",
                table: "StockTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_ProductId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "ProductId")
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_WarehouseId",
                schema: "Alerts",
                table: "StockAlerts",
                column: "WarehouseId")
                .Annotation("SqlServer:Include", new[] { "Status", "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UK_Stock_Product_Warehouse",
                schema: "Inventory",
                table: "Stock",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrders_Number",
                schema: "Purchasing",
                table: "PurchaseOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_PurchaseOrderItems_PO_Product",
                schema: "Purchasing",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Products_SKU",
                schema: "Inventory",
                table: "Products",
                column: "SKU",
                unique: true);
        }
    }
}
