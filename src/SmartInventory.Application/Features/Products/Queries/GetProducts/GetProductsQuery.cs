using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;

namespace SmartInventory.Application.Features.Products.Queries.GetProducts
{
    public record GetProductsQuery : IQuery<List<ProductDto>>;
}
