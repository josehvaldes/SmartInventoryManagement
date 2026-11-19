using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Tenants.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Tenants.Queries
{
    public record class GetTenantBySlugQuery(string Slug) : IQuery<TenantDto?>{ }
}
