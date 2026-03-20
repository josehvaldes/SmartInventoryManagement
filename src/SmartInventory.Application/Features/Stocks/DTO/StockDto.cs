namespace SmartInventory.Application.Features.Stocks.DTO
{
    public class StockDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }

        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }

        public DateTime? LastStockTakeDate { get; set; } = null!;

    }
}
