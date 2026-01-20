using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// StockAlert entity representing alerts for stock levels in the inventory system.
    /// 
    /// Id: Guid (Primary Key)
    /// ProductId: Guid (Foreign Key to Product)
    /// WarehouseId: Guid (Foreign Key to Warehouse)
    /// AlertType: StockAlertType (enum)
    /// CurrentQuantity: decimal
    /// ThresholdQuantity: decimal (the level that triggered the alert)
    /// Message: string (auto-generated description, max 500 chars)
    /// Severity: AlertSeverity (enum: Low, Medium, High, Critical)
    /// Status: AlertStatus (enum)
    /// CreatedAt: DateTime (UTC)
    /// AcknowledgedAt: DateTime? (optional)
    /// AcknowledgedBy: string (optional, username/user ID)
    /// ResolvedAt: DateTime? (optional)
    /// ResolvedBy: string (optional, username/user ID)
    /// ResolutionNotes: string (optional, max 500 chars)
    /// </summary>
    public class StockAlert
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public StockAlertType AlertType { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal ThresholdQuantity { get; set; }
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public AlertStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;

    }
}
