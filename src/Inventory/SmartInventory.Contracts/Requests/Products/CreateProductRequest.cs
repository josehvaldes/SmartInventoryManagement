using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Products
{
    public class CreateProductRequest
    {
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal MinimumStockLevel { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal ReorderQuantity { get; set; }        
        public decimal? UnitCost { get; set; }
    }
}
