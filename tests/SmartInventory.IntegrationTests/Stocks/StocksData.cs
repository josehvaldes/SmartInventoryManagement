using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.IntegrationTests.Stocks
{
    public static class StocksData
    {
        private static readonly string StocksFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "stocks.json");

        public static List<Stock> LoadStocksFromFile()
        {

            if (string.IsNullOrWhiteSpace(StocksFilePath))
            {
                Console.WriteLine("File path is not provided. Skipping stock loading.");
                throw new Exception("File path is not provided. Skipping stock loading.");
            }
            using (var reader = new StreamReader(StocksFilePath))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Stock>>(json) ?? new List<Stock>();
            }
        }
    }
}
