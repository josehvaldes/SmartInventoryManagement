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
            if (context.Products.Any()) 
            {
                return; // Exit if there are already products in the database
            }

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
                    existingProduct.UpdateDetails(
                        product.Name,
                        product.Description,
                        product.Category,
                        product.UnitOfMeasure,
                        product.MinimumStockLevel,
                        product.ReorderPoint,
                        product.ReorderQuantity,
                        product.UnitCost,
                        product.IsActive,
                        "Seeder");
                }
            }
            // Save changes to the database
            context.SaveChanges();
        }
    }
}
