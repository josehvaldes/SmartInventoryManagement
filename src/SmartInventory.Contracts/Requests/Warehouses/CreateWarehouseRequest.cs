using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Warehouses
{
    public class CreateWarehouseRequest
    {
        public string WarehouseType { get; set; } = string.Empty;


        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Street  { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public decimal? Capacity { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
