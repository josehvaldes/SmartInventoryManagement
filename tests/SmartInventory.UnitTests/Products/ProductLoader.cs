using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.UnitTests.Products
{
    public static class ProductLoader
    {
        private static readonly string ProductsFilePath = "C:\\personal\\_SmartInventoryMgmtSystem\\SmartInventoryManagement\\seeds\\SmartInventory.Seeds\\Data\\products.json";


        public static List<Product> LoadProductsFromFile()
        {
            if (string.IsNullOrWhiteSpace(ProductsFilePath))
            {
                Console.WriteLine("File path is not provided. Skipping product seeding.");
                throw new Exception("File path is not provided. Skipping product seeding.");
            }
            using (var reader = new StreamReader(ProductsFilePath))
            {
                var products = new List<Product>();
                var json = reader.ReadToEnd();
                var seeds = JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
                products.AddRange(seeds);

                return products;
            }
        }

    }
}
