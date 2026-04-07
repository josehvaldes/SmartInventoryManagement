using Microsoft.Extensions.Configuration;
using SmartInventory.Seeds;

Console.WriteLine("SmartInventory Product Seeder");
Console.WriteLine($"Env: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");

// Build configuration — layered: base → environment override → env vars
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("SmartInventoryDb")
    ?? throw new InvalidOperationException("Connection string 'SmartInventoryDb' not found.");

Console.WriteLine("Starting seed...");

var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

var seeders = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    [nameof(ProductSeeder)]            = () => new ProductSeeder(connectionString).Seed(Path.Combine(dataDir, "products.json")),
    [nameof(WarehouseSeeder)]          = () => new WarehouseSeeder(connectionString).Seed(Path.Combine(dataDir, "warehouses.json")),
    [nameof(StockSeeder)]              = () => new StockSeeder(connectionString).Seed(Path.Combine(dataDir, "stocks.json")),
    [nameof(UserSeeder)]               = () => new UserSeeder(connectionString).Seed(Path.Combine(dataDir, "users.json")),
    [nameof(StockTransactionSeeder)]   = () => new StockTransactionSeeder(connectionString).Seed(Path.Combine(dataDir, "stock_transactions.json")),
    [nameof(SupplierSeeder)]           = () => new SupplierSeeder(connectionString).Seed(Path.Combine(dataDir, "suppliers.json")),
    [nameof(PurchaseOrderSeeder)]      = () => new PurchaseOrderSeeder(connectionString).Seed(Path.Combine(dataDir, "purchase_orders.json")),
    [nameof(PurchaseOrderItemsSeeder)] = () => new PurchaseOrderItemsSeeder(connectionString).Seed(Path.Combine(dataDir, "purchase_order_items.json")),
};

if (args.Length == 0)
{
    Console.WriteLine("No command provided. Seeding all by default...");
    await Task.WhenAll(seeders.Values.Select(seed => seed()));
}
else
{
    var command = args[0];
    if (seeders.TryGetValue(command, out var seed))
    {
        await seed();
    }
    else
    {
        Console.WriteLine($"Unknown command: '{command}'. Available seeders: {string.Join(", ", seeders.Keys)}");
    }
}

Console.WriteLine("Seed completed.");
