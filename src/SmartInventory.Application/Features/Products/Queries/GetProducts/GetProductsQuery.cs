using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.DTOs;

namespace SmartInventory.Application.Features.Products.Queries.GetProducts
{
    public record GetProductsQuery : IQuery<List<ProductDto>>;
}
