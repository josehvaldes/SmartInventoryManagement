using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class PurchaseOrderSeeder : BaseSeeder<PurchaseOrder>
    {
        public PurchaseOrderSeeder(string connectionString) : base(connectionString) { }
        
        protected override void ProcessSeed(SmartInventoryDbContext context, List<PurchaseOrder> seeds)
        {
            Console.WriteLine($"Seeding {seeds.Count} purchase orders...");
            if (context.PurchaseOrders.Any())
            {
                return; // Exit if there are already purchase order items in the database
            }

            try 
            {
                foreach (var purchaseOrder in seeds)
                {
                    var existingPurchaseOrder = context.PurchaseOrders.Find(purchaseOrder.Id);
                    if (existingPurchaseOrder == null)
                    {
                        purchaseOrder.Items = new List<PurchaseOrderItem>(); // Avoid seeding related items in this seeder
                        context.PurchaseOrders.Add(purchaseOrder);
                    }
                    else
                    {
                        existingPurchaseOrder.OrderNumber = purchaseOrder.OrderNumber;
                        existingPurchaseOrder.SupplierId = purchaseOrder.SupplierId;
                        existingPurchaseOrder.OrderDate = purchaseOrder.OrderDate;
                        existingPurchaseOrder.ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate;
                        existingPurchaseOrder.Status = purchaseOrder.Status;
                        existingPurchaseOrder.UpdatedAt = DateTime.UtcNow;
                    }
                }
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error seeding purchase orders: {e.Message}");
                throw new SeederException("An error occurred while seeding purchase orders. See inner exception for details.", e);
            }
        }
    }
}
