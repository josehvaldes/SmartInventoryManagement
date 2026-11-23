using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Products
{
    public class UploadProductRequest
    {
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }
}
