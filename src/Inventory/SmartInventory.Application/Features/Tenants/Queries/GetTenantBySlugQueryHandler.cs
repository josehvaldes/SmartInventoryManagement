using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public class GetTenantBySlugQueryHandler(IAuthDbContext dbContext,
        ICacheKeyFactory cacheKeyFactory, 
        ICacheService cache) : IQueryHandler<GetTenantBySlugQuery, TenantDto?>
    {
        public async Task<TenantDto?> Handle(GetTenantBySlugQuery request, CancellationToken cancellationToken)
        {
            var key = cacheKeyFactory.ByProperty<TenantDto>("Slug", request.Slug);
            var tenantDto = await cache.GetAsync<TenantDto>(key);

            if (tenantDto != null)
            {
                return tenantDto;
            }
            else
            { // negative caching: store a null value for a short duration to prevent repeated database hits for non-existent tenants
                await cache.SetAsync<TenantDto?>(key, null, TimeSpan.FromSeconds(30));
            }

            // If not found in cache, retrieve from database
            var tenant = await dbContext.Tenants.AsNoTracking().Where(t => t.Slug == request.Slug).FirstOrDefaultAsync(cancellationToken);
            if (tenant == null)
            {
                return null;
            }

            tenantDto = tenant?.Adapt<TenantDto>();
            if (tenantDto != null)
            {
                await cache.SetAsync(key, tenantDto, TimeSpan.FromSeconds(60));
            }            

            return tenantDto;
        }
    }
}
