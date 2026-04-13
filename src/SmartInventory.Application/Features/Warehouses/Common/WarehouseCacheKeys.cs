using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Warehouses.Common
{
    public static class WarehouseCacheKeys
    {
        public static string ById(Guid warehouseId) => $"Warehouse:{warehouseId}";
        public static string All = "Warehouse:All";
    }
}
