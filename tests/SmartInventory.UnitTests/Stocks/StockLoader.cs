using Newtonsoft.Json;
using SmartInventory.Domain.Entities;

namespace SmartInventory.UnitTests.Stocks
{
    public static class StockLoader
    {
        private static readonly string StocksFilePath = "C:\\personal\\_SmartInventoryMgmtSystem\\SmartInventoryManagement\\seeds\\SmartInventory.Seeds\\Data\\stocks.json";

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
