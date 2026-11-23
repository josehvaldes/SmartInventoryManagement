using Microsoft.EntityFrameworkCore.Migrations;
using SmartInventory.Domain.Entities;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations.Inventory
{
    /// <inheritdoc />
    public partial class TenantBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var tenantId = SystemTenants.LegacyTenantId;

            migrationBuilder.Sql($"""
            UPDATE Inventory.Products
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Purchasing.PurchaseOrders
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Purchasing.PurchaseOrderItems
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.Stock 
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Alerts.StockAlerts
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.StockTransactions
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;
            
            UPDATE Purchasing.Suppliers
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.Warehouses
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Guid? tenantId = null;

            migrationBuilder.Sql($"""
            UPDATE Inventory.Products
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Purchasing.PurchaseOrders
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Purchasing.PurchaseOrderItems
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.Stock 
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Alerts.StockAlerts
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.StockTransactions
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;
            
            UPDATE Purchasing.Suppliers
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            UPDATE Inventory.Warehouses
            SET TenantId = '{tenantId}'
            WHERE TenantId IS NULL;

            """);
        }
    }
}
