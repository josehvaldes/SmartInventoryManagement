using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class StockTransactionSeeder: BaseSeeder<StockTransaction>
    {
        public StockTransactionSeeder(string connectionString) : base(connectionString) { }

        protected override void ProcessSeed(SmartInventoryDbContext context, List<StockTransaction> seeds) 
        {
            foreach (var transaction in seeds) 
            {
                if (!context.StockTransactions.Any(t => t.Id == transaction.Id))
                {
                    context.StockTransactions.Add(transaction);
                }
                else 
                {
                    Console.WriteLine($"Stock transaction with ID {transaction.Id} already exists. Skipping.");
                }
            }
            context.SaveChanges();
        }
    }
}
