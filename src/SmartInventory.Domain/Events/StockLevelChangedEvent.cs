using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Events
{
    public class StockLevelChangedEvent : IDomainEvent
    {
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }

    }
}
