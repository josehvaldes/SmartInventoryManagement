using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;
using SmartInventory.Domain.Enums;
using SmartInventory.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public record GetTenantMembershipsQuery(Guid TenantId, Guid UserId) : IQuery<TenantMembershipDto?>
    {
    }
}
