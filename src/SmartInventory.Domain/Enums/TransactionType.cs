using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Specifies the type of transaction affecting inventory stock levels. 
    /// </summary>
    public enum TransactionType
    {
        Receipt = 1,        // Receiving stock (increases quantity)
        Issue = 2,          // Issuing/selling stock (decreases quantity)
        Adjustment = 3,     // Manual adjustment (can be +/-)
        Transfer = 4,       // Transfer between warehouses
        Return = 5,         // Return to supplier (decreases quantity)
        Damage = 6,         // Damaged goods write-off (decreases quantity)
        StockTake = 7       // Physical count adjustment
    }
}
