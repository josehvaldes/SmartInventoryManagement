using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Represents an item in a purchase order.
    /// Id: Guid(Primary Key)
    /// PurchaseOrderId: Guid(Foreign Key to PurchaseOrder)
    /// ProductId: Guid(Foreign Key to Product)
    /// Quantity: decimal (must be > 0)
    /// UnitCost: decimal (must be >= 0)
    /// TotalCost: decimal (computed: Quantity* UnitCost)
    /// ReceivedQuantity: decimal (default: 0)
    /// Notes: string (optional, max 500 chars)
    /// </summary>
    public class PurchaseOrderItem
    {
        

        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost
        {
            get
            {
                return Quantity * UnitCost;
            }
        }
        public decimal ReceivedQuantity { get; set; } = 0;
        public string Notes { get; set; } = string.Empty;

    }
}
