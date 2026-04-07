using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class PurchaseOrderItemsSeeder : BaseSeeder<PurchaseOrderItem>
    {
        public PurchaseOrderItemsSeeder(string connectionString) : base(connectionString) { }
        protected override void ProcessSeed(SmartInventoryDbContext context, List<PurchaseOrderItem> seeds)
        {
            Console.WriteLine($"Seeding {seeds.Count} purchase order items...");
            if (context.PurchaseOrderItems.Any())
            {
                return; // Exit if there are already purchase order items in the database
            }

            try
            {

                foreach (var purchaseOrderItem in seeds)
                {
                    var existingPurchaseOrderItem = context.PurchaseOrderItems.Find(purchaseOrderItem.Id);
                    if (existingPurchaseOrderItem == null)
                    {
                        context.PurchaseOrderItems.Add(purchaseOrderItem);
                    }
                    else
                    {
                        existingPurchaseOrderItem.PurchaseOrderId = purchaseOrderItem.PurchaseOrderId;
                        existingPurchaseOrderItem.ProductId = purchaseOrderItem.ProductId;
                        existingPurchaseOrderItem.Quantity = purchaseOrderItem.Quantity;
                        existingPurchaseOrderItem.UnitCost = purchaseOrderItem.UnitCost;
                        existingPurchaseOrderItem.TotalCost = purchaseOrderItem.TotalCost;
                        existingPurchaseOrderItem.ReceivedQuantity = purchaseOrderItem.ReceivedQuantity;
                        existingPurchaseOrderItem.Notes = purchaseOrderItem.Notes;
                    }
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding purchase order items: {ex.Message}");
                throw new SeederException("An error occurred while seeding purchase order items. See inner exception for details.", ex);
            }
        }
    }
}
