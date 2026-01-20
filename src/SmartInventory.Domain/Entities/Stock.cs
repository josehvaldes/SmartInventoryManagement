using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Stock entity representing inventory levels for products in warehouses.
    /// 
    /// Id: Guid (Primary Key)
    /// ProductId: Guid (Foreign Key to Product)
    /// WarehouseId: Guid (Foreign Key to Warehouse)
    /// QuantityOnHand: decimal (must be >= 0)
    /// QuantityReserved: decimal (must be >= 0, for future orders/holds)
    /// QuantityAvailable: decimal (computed: QuantityOnHand /// QuantityReserved)
    /// LastStockTakeDate: DateTime? (optional, last physical count date)
    /// LastUpdatedAt: DateTime (UTC)
    /// LastTransactionId: Guid? (reference to last transaction)
    /// </summary>
    public class Stock
    {
        

        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable
        {
            get
            {
                return QuantityOnHand - QuantityReserved;
            }
        }
        public decimal LastStockTakeDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastTransactionId { get; set; }
    }
}
