using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Events
{
    /// <summary>
    /// 
    /// TransactionId: Guid
    /// TransactionType: TransactionType
    /// ProductId: Guid
    /// WarehouseId: Guid
    /// Quantity: decimal
    /// CreatedAt: DateTime
    /// </summary>
    public class StockTransactionCreatedEvent
    {
        public Guid TransactionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }

        public StockTransactionCreatedEvent() { }
    }
}
