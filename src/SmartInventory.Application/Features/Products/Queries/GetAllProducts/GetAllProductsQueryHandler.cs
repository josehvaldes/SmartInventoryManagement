using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Models;
using SmartInventory.Application.Features.Products.DTO;

namespace SmartInventory.Application.Features.Products.Queries.GetAllProducts
{
    public class GetAllProductsQueryHandler(IApplicationDbContext db, ICacheService cache)
        : IQueryHandler<GetAllProductsQuery, PagedResult<ProductDto>>
    {
        public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
        {
            var totalCount = await db.Products.CountAsync(cancellationToken);

            var products = await db.Products
                .AsNoTracking()
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var productDtos = products.Adapt<List<ProductDto>>();
            var tasks = productDtos.Select(x => cache.SetAsync(CacheKeys<ProductDto>.ById(x.Id), x, TimeSpan.FromMinutes(5)));
            await Task.WhenAll(tasks);

            return new PagedResult<ProductDto>(productDtos, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
