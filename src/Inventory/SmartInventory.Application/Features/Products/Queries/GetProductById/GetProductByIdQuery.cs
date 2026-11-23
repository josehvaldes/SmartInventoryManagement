using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Queries.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto> { }
}
