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

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Console.WriteLine("Starting seed...");


var command = "WarehouseSeeder"; //"StockSeeder"

if (command == "ProductSeeder")
{
    var productsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
    var seeder = new ProductSeeder(connectionString);
    await seeder.Seed(productsFilePath);
}
else if (command == "WarehouseSeeder")
{
    var stocksFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "warehouses.json");
    var seeder = new WarehouseSeeder(connectionString);
    await seeder.Seed(stocksFilePath);
}

Console.WriteLine("Seed completed.");
