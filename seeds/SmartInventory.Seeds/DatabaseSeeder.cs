using SmartInventory.Identity.Infrastructure.Data.Context;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Seeds
{
    public static class DatabaseSeeder
    {

        public static async Task SeedAllAsync(AuthDbContext authDbContext, SmartInventoryDbContext smartInventoryDbContext) 
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

            var seeders = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(UserSeeder)] = () => new UserSeeder(authDbContext).Seed(Path.Combine(dataDir, "users.json")),
                [nameof(TenantSeeder)] = () => new TenantSeeder(authDbContext).Seed(Path.Combine(dataDir, "tenants.json")),
                [nameof(TenantMembershipSeeder)] = () => new TenantMembershipSeeder(authDbContext).Seed(Path.Combine(dataDir, "tenant_memberships.json")),
                [nameof(ProductSeeder)] = () => new ProductSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "products.json")),
                [nameof(WarehouseSeeder)] = () => new WarehouseSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "warehouses.json")),
                [nameof(StockSeeder)] = () => new StockSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "stocks.json")),                
                [nameof(StockTransactionSeeder)] = () => new StockTransactionSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "stock_transactions.json")),
                [nameof(SupplierSeeder)] = () => new SupplierSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "suppliers.json")),
                [nameof(PurchaseOrderSeeder)] = () => new PurchaseOrderSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "purchase_orders.json")),
                [nameof(PurchaseOrderItemsSeeder)] = () => new PurchaseOrderItemsSeeder(smartInventoryDbContext).Seed(Path.Combine(dataDir, "purchase_order_items.json")),
            };

            foreach (var seeder in seeders)
            {
                Console.WriteLine($"Seeding {seeder.Key}...");

                try
                {
                    await seeder.Value();
                    Console.WriteLine($"{seeder.Key} seeded successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding {seeder.Key}: {ex.Message}");
                }

            }
        }
    }
}
