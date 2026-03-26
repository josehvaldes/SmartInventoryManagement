using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Products
{
    public class GetUploadUrlRequest
    {
        public string ProductSku { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
