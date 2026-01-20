using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// 
    /// Id: Guid (Primary Key)
    /// Code: string (unique, max 20 chars, e.g., "WH-001", "WH-MAIN")
    /// Name: string (required, max 200 chars)
    /// Address: Address (value object)
    /// WarehouseType: WarehouseType (enum)
    /// Capacity: decimal? (optional, in cubic meters or square feet)
    /// ManagerName: string (optional, max 100 chars)
    /// ManagerEmail: string (optional, max 100 chars)
    /// ManagerPhone: string (optional, max 20 chars)
    /// IsActive: bool (default: true)
    /// CreatedAt: DateTime (UTC)
    /// UpdatedAt: DateTime (UTC)
    /// </summary>
    public class Warehouse
    {

        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Address? Address { get; set; } = null;
        public WarehouseType WarehouseType { get; set; }
        public decimal? Capacity { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        }
}
