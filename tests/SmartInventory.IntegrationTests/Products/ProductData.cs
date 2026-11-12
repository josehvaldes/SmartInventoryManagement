using Newtonsoft.Json;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.IntegrationTests.Common;

namespace SmartInventory.IntegrationTests.Products
{
    public static class ProductData
    { 

        private static readonly string ProductsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");

        private static readonly JsonSerializerSettings _deserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static List<Product> LoadProductsFromFile()
        {
            if (string.IsNullOrWhiteSpace(ProductsFilePath) || !File.Exists(ProductsFilePath))
                throw new FileNotFoundException("File path is not provided or file does not exist. Skipping product seeding.");

            using (var reader = new StreamReader(ProductsFilePath))
            {
                var products = new List<Product>();
                var json = reader.ReadToEnd();
                var seeds = JsonConvert.DeserializeObject<List<Product>>(json, _deserializationSettings) ?? new List<Product>();
                products.AddRange(seeds);

                return products;
            }
        }

        public static readonly CreateProductRequest Product1 = new CreateProductRequest
        {
            SKU = "ITM-USB-C-001",
            Name = "USB-C Charging Cable 2m",
            Description = "Braided USB-C cable for daily use",
            Category = ProductCategory.Electronics.ToString(),
            UnitOfMeasure = UnitOfMeasure.Piece.ToString(),
            MinimumStockLevel = 5,
            ReorderPoint = 8,
            ReorderQuantity = 20,
            UnitCost = 9.99m,
        };

        public static readonly List<Product> Products = new List<Product>
        {
            Product.Create(
                sku: "ITM-USB-C-001",
                name: "USB-C Charging Cable 2m",
                description: "Braided USB-C cable for daily use",
                category: ProductCategory.Electronics,
                unitOfMeasure: UnitOfMeasure.Piece,
                minimumStockLevel: 5,
                reorderPoint: 8,
                reorderQuantity: 20,
                unitCost: 9.99m,
                createdBy: "integration-test"),
            Product.Create(
                sku: "ITM-GLOVES-001",
                name: "Safety Gloves",
                description: "Nitrile disposable safety gloves",
                category: ProductCategory.Safety,
                unitOfMeasure: UnitOfMeasure.Box,
                minimumStockLevel: 3,
                reorderPoint: 5,
                reorderQuantity: 10,
                unitCost: 14.50m,
                createdBy: "integration-test")
        };
    }
}
