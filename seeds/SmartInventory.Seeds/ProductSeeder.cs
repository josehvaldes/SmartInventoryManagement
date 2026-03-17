using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Seeds
{
    public class ProductSeeder
    {
        private readonly string _connectionString = string.Empty;

        public ProductSeeder(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Seed(string filePath) 
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
            {
                Console.WriteLine("File path is not provided. Skipping product seeding.");
                return;
            }

            // open the file and read the products
            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
                
                var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options;
                
                using (var context = new SmartInventoryDbContext(options))
                {
                    foreach (var product in products)
                    {
                        // Check if the product already exists by SKU
                        var existingProduct = await context.Products.FindAsync(product.Id);
                        if (existingProduct == null)
                        {
                            // If it doesn't exist, add it to the context
                            context.Products.Add(product);
                        }
                        else
                        {
                            // Optionally, update existing product details here
                            existingProduct.Name = product.Name;
                            existingProduct.Description = product.Description;
                            existingProduct.Category = product.Category;
                            existingProduct.UnitOfMeasure = product.UnitOfMeasure;
                            existingProduct.MinimumStockLevel = product.MinimumStockLevel;
                            existingProduct.ReorderPoint = product.ReorderPoint;
                            existingProduct.ReorderQuantity = product.ReorderQuantity;
                            existingProduct.UnitCost = product.UnitCost;
                            existingProduct.IsActive = product.IsActive;
                            existingProduct.UpdatedAt = DateTime.UtcNow;
                            existingProduct.UpdatedBy = "Seeder";
                        }
                    }
                    // Save changes to the database
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
