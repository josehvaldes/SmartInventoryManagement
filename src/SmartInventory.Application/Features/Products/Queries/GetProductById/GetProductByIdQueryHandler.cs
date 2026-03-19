using Mapster;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetProductByIdQuery, ProductDto>
    {
        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var key = $"product:{request.Id}";

            var product = await cache.GetAsync<ProductDto>(key);

            if (product != null)
                return product;

            var entity = await db.Products.FindAsync(request.Id, cancellationToken);

            if (entity == null)
                throw EntityNotFoundException.For<Product>(request.Id);

            var dto = entity.Adapt<ProductDto>();
            await cache.SetAsync(key, dto, TimeSpan.FromMinutes(5));
            return dto;
        }
    }
}
