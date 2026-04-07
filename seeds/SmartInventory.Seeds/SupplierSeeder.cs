using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class SupplierSeeder: BaseSeeder<Supplier>
    {
        public SupplierSeeder(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ProcessSeed(SmartInventoryDbContext context, List<Supplier> seeds) 
        {
            Console.WriteLine($"Seeding {seeds.Count} suppliers...");

            if (context.Suppliers.Any())
            {
                return; // Exit if there are already suppliers in the database
            }

            foreach (var supplier in seeds)
            {
                var existingSupplier = context.Suppliers.Find(supplier.Id);
                if (existingSupplier == null)
                {
                    context.Suppliers.Add(supplier);
                }
                else
                {
                    existingSupplier.Code = supplier.Code;
                    existingSupplier.Name = supplier.Name;
                    existingSupplier.ContactPerson = supplier.ContactPerson;
                    existingSupplier.Email = supplier.Email;
                    existingSupplier.Phone = supplier.Phone;
                    existingSupplier.Address = supplier.Address;
                    existingSupplier.PaymentTerms = supplier.PaymentTerms;
                    existingSupplier.LeadTimeDays = supplier.LeadTimeDays;
                    existingSupplier.MinimumOrderValue = supplier.MinimumOrderValue;
                    existingSupplier.Rating = supplier.Rating;
                    existingSupplier.IsActive = supplier.IsActive;
    
                    existingSupplier.UpdatedAt = DateTime.UtcNow;
                }
            }
            context.SaveChanges();
        }
    }
}
