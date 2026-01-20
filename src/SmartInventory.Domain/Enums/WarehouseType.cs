using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Represents the various types of warehouses in the inventory system.
    /// </summary>
    public enum WarehouseType
    {
        Main = 1,           // Primary warehouse
        Regional = 2,       // Regional distribution center
        Transit = 3,        // Temporary/in-transit storage
        ReturnCenter = 4,   // For returns and refurbishment
        Virtual = 5         // Logical warehouse (no physical location)
    }
}
