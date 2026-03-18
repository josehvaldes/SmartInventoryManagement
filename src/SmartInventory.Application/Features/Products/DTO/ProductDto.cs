using SmartInventory.Domain.Enums;

namespace SmartInventory.Application.Features.Products.DTO
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProductCategory Category { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
