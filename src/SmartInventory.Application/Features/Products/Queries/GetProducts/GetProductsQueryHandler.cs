using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;

namespace SmartInventory.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler(IApplicationDbContext db, ICacheService cache)
        : IQueryHandler<GetProductsQuery, List<ProductDto>>
    {
        public async Task<List<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            var products = await db.Products
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productDtos = products.Adapt<List<ProductDto>>();
            var tasks = productDtos.Select(x => cache.SetAsync(CacheKeys<ProductDto>.ById(x.Id), x, TimeSpan.FromMinutes(5)));
            await Task.WhenAll(tasks);
            return productDtos;
        }
    }
}
