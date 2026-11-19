using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
        Guid ResolvedTenantId { get; } // The tenant ID that has been resolved from the request context (e.g., from a slug or header). This may differ from TenantId if the user is operating cross-tenant.
        bool IsAvailable { get; }   // false for system/admin/health-check paths
        bool IsTenantScoped { get; } // false when explicitly operating cross-tenant (operator/system)
        string Role { get; } // The role of the current user in the tenant context
    }
}
