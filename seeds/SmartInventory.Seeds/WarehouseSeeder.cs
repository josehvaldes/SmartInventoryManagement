using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SmartInventory.Seeds
{
    public class WarehouseSeeder
    {
        private string _connectionString = string.Empty;

        public WarehouseSeeder(string connectionString)
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

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var warehouses = JsonConvert.DeserializeObject<List<Warehouse>>(json) ?? new List<Warehouse>();

                var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options;

                using (var context = new SmartInventoryDbContext(options)) 
                {
                    foreach (var warehouse in warehouses)
                    {
                        var existingWarehouse = await context.Warehouses.FindAsync(warehouse.Id);
                        if (existingWarehouse == null)
                        {
                            context.Warehouses.Add(warehouse);
                        }
                        else
                        {
                            existingWarehouse.Name = warehouse.Name;
                            existingWarehouse.Code = warehouse.Code;
                            existingWarehouse.Address = warehouse.Address;
                            existingWarehouse.WarehouseType = warehouse.WarehouseType;
                            existingWarehouse.Capacity = warehouse.Capacity;
                            existingWarehouse.ManagerEmail = warehouse.ManagerEmail;
                            existingWarehouse.ManagerPhone = warehouse.ManagerPhone;
                            existingWarehouse.ManagerName = warehouse.ManagerName;
                            existingWarehouse.IsActive = warehouse.IsActive;
                            existingWarehouse.UpdatedAt = DateTime.UtcNow;

                        }
                    }
                    // Save changes to the database
                    await context.SaveChangesAsync();
                }                
            }
        }
    }
}
