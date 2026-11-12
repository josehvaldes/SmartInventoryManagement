using Newtonsoft.Json;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;

namespace SmartInventory.UnitTests.Warehouses
{
    public class WarehouseLoader
    {
        private static readonly string WarehousesFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "warehouses.json");
        private static readonly JsonSerializerSettings _deserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static List<Warehouse> LoadWarehouses()
        {
            if (string.IsNullOrEmpty(WarehousesFilePath) || !File.Exists(WarehousesFilePath))
                throw new FileNotFoundException($"The file '{WarehousesFilePath}' was not found.");

            var json = File.ReadAllText(WarehousesFilePath);
            return JsonConvert.DeserializeObject<List<Warehouse>>(json, _deserializationSettings) ?? new List<Warehouse>();
        }
    }
}
