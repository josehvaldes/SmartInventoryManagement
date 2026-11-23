using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Models;
using SmartInventory.Application.Features.Products.DTO;

namespace SmartInventory.Application.Features.Products.Queries.GetAllProducts
{
    public record GetAllProductsQuery(int PageNumber, int PageSize) : IQuery<PagedResult<ProductDto>>;
}
