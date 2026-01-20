using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Events
{
    /// <summary>
    /// Represents an event that occurs when a product's inventory level reaches its reorder point.
    /// 
    /// ProductId: Guid
    /// WarehouseId: Guid
    /// CurrentQuantity: decimal
    /// ReorderPoint: decimal
    /// ReorderQuantity: decimal
    /// OccurredAt: DateTime
    /// </summary>
    public class ProductReorderPointReachedEvent
    {
        public Guid ProductId { get; init; }
        public Guid WarehouseId { get; init; }
        public decimal CurrentQuantity { get; init; }
        public decimal TotalQuantity { get; init; }
        public decimal ReorderPoint { get; init; }
        public decimal ReorderQuantity { get; init; }
        public DateTime OccurredAt { get; init; }

        public ProductReorderPointReachedEvent() { }
    }
}
