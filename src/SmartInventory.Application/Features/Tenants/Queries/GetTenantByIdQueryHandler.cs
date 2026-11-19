using Mapster;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public class GetTenantByIdQueryHandler(IAuthDbContext dbContext, ICacheService cacheService) : IQueryHandler<GetTenantByIdQuery, TenantDto?>
    {
        public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            var key = CacheKeys<TenantDto>.ById(request.TenantId);
            var cachedTenant = await cacheService.GetAsync<TenantDto>(key);
            if (cachedTenant != null)
            {
                return cachedTenant;
            }
            
            var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);

            if (tenant != null)
            {
                var tenantDto = tenant.Adapt<TenantDto>();
                await cacheService.SetAsync(key, tenantDto);
            }
            return null;
        }
    }
}
