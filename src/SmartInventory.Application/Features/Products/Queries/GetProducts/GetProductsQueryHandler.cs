using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;

namespace SmartInventory.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler(IApplicationDbContext db)
        : IQueryHandler<GetProductsQuery, List<ProductDto>>
    {
        public async Task<List<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            var products = await db.Products
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return products.Adapt<List<ProductDto>>();
        }
    }
}
