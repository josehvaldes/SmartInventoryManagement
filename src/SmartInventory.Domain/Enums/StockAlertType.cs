using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Specifies the type of inventory alert for a stock item.
    /// </summary>
    /// <remarks>Use this enumeration to identify the reason for a stock-related alert, such as low inventory,
    /// overstock, or data integrity issues. The alert type can help determine the appropriate action to take in
    /// inventory management workflows.</remarks>
    public enum StockAlertType
    {
        LowStock = 1,           // Below minimum stock level
        BelowReorderPoint = 2,  // Below reorder point
        Overstock = 3,          // Excessive stock (optional)
        NoMovement = 4,         // No transactions in X days (optional)
        NegativeStock = 5       // Data integrity issue (should never happen)
    }
}
