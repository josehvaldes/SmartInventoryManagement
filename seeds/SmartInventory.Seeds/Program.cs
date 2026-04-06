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

if (args.Length == 0)
    {
    Console.WriteLine("No command provided. Please specify a seeding command (e.g., ProductSeeder, WarehouseSeeder, StockSeeder).");
    return;
}

var command = args[0];

switch (command)
{
    case "ProductSeeder":
        var productsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
        var productSeeder = new ProductSeeder(connectionString);
        await productSeeder.Seed(productsFilePath);
        break;
    case "WarehouseSeeder":
        var stocksFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "warehouses.json");
        var warehouseSeeder = new WarehouseSeeder(connectionString);
        await warehouseSeeder.Seed(stocksFilePath);
        break;
    case "StockSeeder":
        var stockFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "stocks.json");
        var stockSeeder = new StockSeeder(connectionString);
        await stockSeeder.Seed(stockFilePath);
        break;
    case "UserSeeder":
        var usersFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "users.json");
        var userSeeder = new UserSeeder(connectionString);
        await userSeeder.Seed(usersFilePath);
        break;
    case "StockTransactions":
        var transactionsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "stock_transactions.json");
        var transactionSeeder = new StockTransactionSeeder(connectionString);
        await transactionSeeder.Seed(transactionsFilePath);
        break;
    case "SupplierSeeder":
        var suppliersFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "suppliers.json");
        var supplierSeeder = new SupplierSeeder(connectionString);
        await supplierSeeder.Seed(suppliersFilePath);
        break;
    case "PurchaseOrderSeeder":
        var purchaseOrdersFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "purchase_orders.json");
        var purchaseOrderSeeder = new PurchaseOrderSeeder(connectionString);
        await purchaseOrderSeeder.Seed(purchaseOrdersFilePath);
        break;
     case "PurchaseOrderItemsSeeder":
        var purchaseOrderItemsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "purchase_order_items.json");
        var purchaseOrderItemsSeeder = new PurchaseOrderItemsSeeder(connectionString);
        await purchaseOrderItemsSeeder.Seed(purchaseOrderItemsFilePath);
        break;
    default:
        Console.WriteLine($"Unknown command: {command}");
        break;
}

Console.WriteLine("Seed completed.");
