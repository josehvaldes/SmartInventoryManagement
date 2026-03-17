using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Seeds
{
    public class StockSeeder
    {
        private readonly string _filePath = string.Empty;
        private readonly string _connectionString = string.Empty;

        public StockSeeder(string connectionString)
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
                var stocks = JsonConvert.DeserializeObject<List<Stock>>(json) ?? new List<Stock>();

                var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                                .UseSqlServer(_connectionString)
                                .Options;

                using (var context = new SmartInventoryDbContext(options))
                { 
                    foreach(var stock in stocks)
                    {
                        var existingStock = await context.Stocks.FindAsync(stock.Id);
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
                    await context.SaveChangesAsync();
                }

            }
        }

    }
}
