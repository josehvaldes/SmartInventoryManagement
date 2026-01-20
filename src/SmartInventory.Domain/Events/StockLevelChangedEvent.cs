using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Events
{
    internal class StockLevelChangedEvent
    {
        /*
         * ProductId: Guid
         * WarehouseId: Guid
         * OldQuantity: decimal
         * NewQuantity: decimal
         * ChangeReason: string
         * OccurredAt: DateTime
         */
        public Guid ProductId { get; }
        public Guid WarehouseId { get; }
        public decimal OldQuantity { get; }
        public decimal NewQuantity { get; }
        public string ChangeReason { get; } = string.Empty;
        public DateTime OccurredAt { get; }
    }
}
