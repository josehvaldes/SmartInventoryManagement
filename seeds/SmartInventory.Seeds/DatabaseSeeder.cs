using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public static class DatabaseSeeder
    {

        public static async Task SeedAllAsync(string connectionString) 
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

            var seeders = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(UserSeeder)] = () => new UserSeeder(connectionString).Seed(Path.Combine(dataDir, "users.json")),
                [nameof(ProductSeeder)] = () => new ProductSeeder(connectionString).Seed(Path.Combine(dataDir, "products.json")),
                [nameof(WarehouseSeeder)] = () => new WarehouseSeeder(connectionString).Seed(Path.Combine(dataDir, "warehouses.json")),
                [nameof(StockSeeder)] = () => new StockSeeder(connectionString).Seed(Path.Combine(dataDir, "stocks.json")),                
                [nameof(StockTransactionSeeder)] = () => new StockTransactionSeeder(connectionString).Seed(Path.Combine(dataDir, "stock_transactions.json")),
                [nameof(SupplierSeeder)] = () => new SupplierSeeder(connectionString).Seed(Path.Combine(dataDir, "suppliers.json")),
                [nameof(PurchaseOrderSeeder)] = () => new PurchaseOrderSeeder(connectionString).Seed(Path.Combine(dataDir, "purchase_orders.json")),
                [nameof(PurchaseOrderItemsSeeder)] = () => new PurchaseOrderItemsSeeder(connectionString).Seed(Path.Combine(dataDir, "purchase_order_items.json")),
            };

            foreach (var seeder in seeders)
            {
                Console.WriteLine($"Seeding {seeder.Key}...");
                await seeder.Value();
                Console.WriteLine($"{seeder.Key} seeded successfully.");
            }
        }
    }
}
