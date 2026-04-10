using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using System.Reflection;


namespace SmartInventory.UnitTests.Products
{
    public static class ProductLoader
    {
        private static readonly string ProductsFilePath = "C:\\personal\\_SmartInventoryMgmtSystem\\SmartInventoryManagement\\seeds\\SmartInventory.Seeds\\Data\\products.json";

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
    }

}
