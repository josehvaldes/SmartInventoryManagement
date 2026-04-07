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
    public class WarehouseSeeder : BaseSeeder<Warehouse>
    {
        public WarehouseSeeder(string connectionString): base(connectionString) { }

        protected override void ProcessSeed(SmartInventoryDbContext context, List<Warehouse> seeds)
        {
            if (context.Warehouses.Any())
            {
                return; // Exit if there are already users in the database
            }

            foreach (var warehouse in seeds)
            {
                var existingWarehouse = context.Warehouses.Find(warehouse.Id);
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
            context.SaveChanges();            
        }
    }
}
