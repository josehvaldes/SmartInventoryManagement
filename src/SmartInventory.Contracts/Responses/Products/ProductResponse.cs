namespace SmartInventory.Contracts.Responses.Products
{
    public record ProductResponse
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal? UnitCost { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
