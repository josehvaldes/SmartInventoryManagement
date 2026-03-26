using SmartInventory.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.GetUploadUrl
{
    public record GetUploadUrlCommand(string ProductSku, string FileName, string ContentType) : ICommand<string>
    {
    }
}
