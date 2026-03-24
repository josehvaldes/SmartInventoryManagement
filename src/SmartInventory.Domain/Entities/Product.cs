using SmartInventory.Domain.Enums;

namespace SmartInventory.Domain.Entities
{
    public class Product
    {
        private Product() { } // For EF Core materialisation

        public Guid Id { get; private set; }
        public string SKU { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public ProductCategory Category { get; private set; }
        public UnitOfMeasure UnitOfMeasure { get; private set; }
        public decimal MinimumStockLevel { get; private set; }
        public decimal ReorderPoint { get; private set; }
        public decimal ReorderQuantity { get; private set; }
        public decimal? UnitCost { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public string UpdatedBy { get; private set; } = string.Empty;

        /// <summary>
        /// Creates a new product, enforcing all creation-time invariants.
        /// </summary>
        public static Product Create(
            string sku,
            string name,
            string description,
            ProductCategory category,
            UnitOfMeasure unitOfMeasure,
            decimal minimumStockLevel,
            decimal reorderPoint,
            decimal reorderQuantity,
            decimal? unitCost,
            string createdBy)
        {
            return new Product
            {
                Id                = Guid.NewGuid(),
                SKU               = sku,
                Name              = name,
                Description       = description,
                Category          = category,
                UnitOfMeasure     = unitOfMeasure,
                MinimumStockLevel = minimumStockLevel,
                ReorderPoint      = reorderPoint,
                ReorderQuantity   = reorderQuantity,
                UnitCost          = unitCost,
                IsActive          = true,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow,
                CreatedBy         = createdBy,
                UpdatedBy         = createdBy
            };
        }

        /// <summary>
        /// Updates mutable product details and stamps the audit fields.
        /// </summary>
        public void UpdateDetails(
            string name,
            string description,
            ProductCategory category,
            UnitOfMeasure unitOfMeasure,
            decimal minimumStockLevel,
            decimal reorderPoint,
            decimal reorderQuantity,
            decimal? unitCost,
            bool isActive,
            string updatedBy)
        {
            Name              = name;
            Description       = description;
            Category          = category;
            UnitOfMeasure     = unitOfMeasure;
            MinimumStockLevel = minimumStockLevel;
            ReorderPoint      = reorderPoint;
            ReorderQuantity   = reorderQuantity;
            UnitCost          = unitCost;
            IsActive          = isActive;
            UpdatedBy         = updatedBy;
            UpdatedAt         = DateTime.UtcNow;
        }

        public bool ValidateStockLevel(decimal currentStock) => currentStock >= MinimumStockLevel;
    }
}
