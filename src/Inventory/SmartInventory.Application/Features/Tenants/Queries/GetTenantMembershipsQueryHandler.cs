using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;
using SmartInventory.Identity.Abstractions.Persistence;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public class GetTenantMembershipsQueryHandler(IAuthDbContext dbContext) : IQueryHandler<GetTenantMembershipsQuery, TenantMembershipDto?>
    {
        public async Task<TenantMembershipDto?> Handle(GetTenantMembershipsQuery request, CancellationToken cancellationToken)
        {
            var memberships = await dbContext.TenantMemberships.AsNoTracking()
                .Where(tm => tm.TenantId == request.TenantId && tm.UserId == request.UserId)
                .Include(tm => tm.Tenant).FirstOrDefaultAsync(cancellationToken);

            var membershipDto = memberships?.Adapt<TenantMembershipDto>();
            return membershipDto;
        }
    }
}
