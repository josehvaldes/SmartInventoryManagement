
namespace SmartInventory.Application.Common.IntegrationEvents
{
    public class StockLevelChangedIntegrationEvent : IIntegrationEvent
    {
        public Guid ProductId { get; }
        public Guid WarehouseId { get; }
        public decimal OldQuantity { get; }
        public decimal NewQuantity { get; }
        public string ChangeReason { get; } = string.Empty;
        public DateTime OccurredAt { get; }

        public StockLevelChangedIntegrationEvent(
            Guid productId,
            Guid warehouseId,
            decimal oldQuantity,
            decimal newQuantity,
            string changeReason,
            DateTime occurredAt
            ) 
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            OldQuantity = oldQuantity;
            NewQuantity = newQuantity;
            ChangeReason = changeReason;
            OccurredAt = occurredAt;
        }
    }
}
