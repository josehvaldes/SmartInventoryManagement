using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Seeds
{
    public class ProductSeeder: BaseSeeder<Product>
    {
        
        public ProductSeeder(string connectionString): base(connectionString) { }

        protected override void ProcessSeed(SmartInventoryDbContext context, List<Product> seeds) 
        {
            foreach (var product in seeds)
            {
                // Check if the product already exists by SKU
                var existingProduct = context.Products.Find(product.Id);
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
            context.SaveChanges();
        }
    }
}
