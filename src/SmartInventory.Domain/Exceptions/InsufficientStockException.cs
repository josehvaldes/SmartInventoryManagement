using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when there is insufficient stock for a product in a warehouse.
    /// - ProductId: Guid
    ///- WarehouseId: Guid
    ///- RequestedQuantity: decimal
    ///- AvailableQuantity: decimal
    ///- Message: "Insufficient stock for product {ProductId} at warehouse {WarehouseId}"
    /// </summary>
    public class InsufficientStockException: Exception
    {
        public Guid ProductId { get; init; }
        public Guid WarehouseId { get; init; }
        public decimal RequestedQuantity { get; init; }
        public decimal AvailableQuantity { get; init; }
        public override string Message => $"Insufficient stock for product {ProductId} at warehouse {WarehouseId}. Requested: {RequestedQuantity}, Available: {AvailableQuantity}";

        public InsufficientStockException(): base() 
        {

        }
    }
}
