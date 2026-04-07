using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class StockSeeder : BaseSeeder<Stock>
    {
        public StockSeeder(string connectionString) : base(connectionString) { }

        protected override void ProcessSeed(SmartInventoryDbContext context, List<Stock> seeds)
        {
            if (context.Stocks.Any())
            {
                return; // Exit if there are already stock items in the database
            }

            foreach (var stock in seeds)
            {
                var existingStock = context.Stocks.Find(stock.Id);
                if (existingStock == null)
                {
                    context.Stocks.Add(stock);
                }
                else
                {
                    existingStock.ProductId = stock.ProductId;
                    existingStock.WarehouseId = stock.WarehouseId;
                    existingStock.QuantityOnHand = stock.QuantityOnHand;
                    existingStock.QuantityReserved = stock.QuantityReserved;
                    existingStock.QuantityAvailable = stock.QuantityAvailable;
                    existingStock.LastStockTakeDate = stock.LastStockTakeDate;
                    existingStock.LastTransactionId = stock.LastTransactionId;

                    existingStock.LastUpdatedAt = DateTime.UtcNow;
                }
            }

            context.SaveChanges();
        }
    }
}
