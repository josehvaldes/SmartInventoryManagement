using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Events
{
    /// <summary>
    /// Provided when a purchase order is received and stock levels need to be updated.
    /// PurchaseOrderId: Guid
    /// OrderNumber: string
    /// SupplierId: Guid
    /// WarehouseId: Guid
    /// TotalItems: int
    /// TotalValue: decimal
    /// ReceivedAt: DateTime
    /// </summary>
    public class PurchaseOrderReceivedEvent
    {
        public Guid PurchaseOrderId { get; init; }
        public string OrderNumber { get; init; }
        public Guid SupplierId { get; init; }
        public Guid WarehouseId { get; init; }
        public int TotalItems { get; init; }
        public decimal TotalValue { get; init; }
        public DateTime ReceivedAt { get; init; }

        public PurchaseOrderReceivedEvent() { }
    }
}
