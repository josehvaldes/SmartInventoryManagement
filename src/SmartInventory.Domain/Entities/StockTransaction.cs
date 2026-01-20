using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// StockTransaction entity representing stock movements in the inventory system.
    /// 
    /// Id: Guid(Primary Key)
    /// TransactionNumber: string (unique, auto-generated, e.g., "TXN-2026-001234")
    /// ProductId: Guid(Foreign Key to Product)
    /// WarehouseId: Guid(Foreign Key to Warehouse)
    /// TransactionType: TransactionType(enum)
    /// Quantity: decimal (can be positive or negative depending on type)
    /// UnitCost: decimal? (optional, cost per unit at transaction time)
    /// TotalCost: decimal (computed: Quantity* UnitCost)
    /// ReferenceNumber: string (optional, max 100 chars* PO number, invoice, etc.)
    /// SourceWarehouseId: Guid? (for transfers, optional)
    /// DestinationWarehouseId: Guid? (for transfers, optional)
    /// Reason: string (optional, max 500 chars)
    /// Notes: string (optional, max 1000 chars)
    /// TransactionDate: DateTime(UTC, when transaction occurred)
    /// CreatedAt: DateTime(UTC, when record was created)
    /// CreatedBy: string (username/user ID)
    /// IsReversed: bool (default: false)
    /// ReversedByTransactionId: Guid? (reference to reversal transaction)
    /// </summary>
    public class StockTransaction
    {
        public Guid Id { get; init; }
        public string TransactionNumber { get; init; } = string.Empty;
        public Guid ProductId { get; init; }
        public Guid WarehouseId { get; init; }
        public TransactionType TransactionType { get; init; }
        public decimal Quantity { get; init; }
        public decimal? UnitCost { get; init; }
        public decimal TotalCost
        {
            get
            {
                return UnitCost.HasValue ? Quantity * UnitCost.Value : 0;
            }
        }
        public string ReferenceNumber { get; init; } = string.Empty;
        public Guid? SourceWarehouseId { get; init; }
        public Guid? DestinationWarehouseId { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public DateTime TransactionDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public bool IsReversed { get; init; } = false;
        public Guid? ReversedByTransactionId { get; init; }
    }
}
