using Microsoft.AspNetCore.Http;
using SmartInventory.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.UploadProduct
{
    public record UploadProductCommand(string productSKU, string productName, IFormFile file, string updatedBy) : ICommand<string>
    {
    }
}
