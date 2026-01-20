using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    /// <summary>
    /// Represents a product in the inventory system.
    //* Id: Guid(Primary Key)
    //* SKU: string (Stock Keeping Unit - unique, max 50 chars)
    //* Name: string (required, max 200 chars)
    //* Description: string (optional, max 1000 chars)
    //* Category: ProductCategory(enum)
    //* UnitOfMeasure: UnitOfMeasure(enum)
    //* MinimumStockLevel: decimal (must be >= 0)
    //* ReorderPoint: decimal (must be >= 0)
    //* ReorderQuantity: decimal (must be > 0)
    //* UnitCost: decimal (optional, for reference)
    //* IsActive: bool (default: true)
    //* CreatedAt: DateTime(UTC)
    //* UpdatedAt: DateTime(UTC)
    //* CreatedBy: string (username/user ID)
    //* UpdatedBy: string (username/user ID)
    /// </summary>
    public class Product
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Validates if the current stock level meets or exceeds the minimum stock level.
        /// </summary>
        /// <param name="currentStock"></param>
        /// <returns>True if the stock level is valid; otherwise, false.</returns>
        public bool ValidateStockLevel(decimal currentStock)
        {
            return currentStock >= MinimumStockLevel;
        }
    }
}
