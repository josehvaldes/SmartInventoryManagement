using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public class GetTenantMembershipsQueryHandler(IAuthDbContext dbContext, ICacheService cacheService ) : IQueryHandler<GetTenantMembershipsQuery, TenantMembershipDto?>
    {
        public async Task<TenantMembershipDto?> Handle(GetTenantMembershipsQuery request, CancellationToken cancellationToken)
        {
            var key = CacheKeys<TenantMembershipDto>.ByTenantAndProperty(request.TenantId, "UserId", request.UserId.ToString());
            var membershipDto = await cacheService.GetAsync<TenantMembershipDto>(key);
            if (membershipDto != null)
            {
                return membershipDto;
            }

            var memberships = await dbContext.TenantMemberships.AsNoTracking()
                .Where(tm => tm.TenantId == request.TenantId && tm.UserId == request.UserId)
                .Include(tm => tm.Tenant).FirstOrDefaultAsync(cancellationToken);

            membershipDto = memberships?.Adapt<TenantMembershipDto>();
            
            if (membershipDto != null)
            {
                // membership cached with tenant
                await cacheService.SetAsync(key, membershipDto, TimeSpan.FromSeconds(30));
            }
            return membershipDto;
        }
    }
}
