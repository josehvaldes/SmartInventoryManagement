using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
        bool IsAvailable { get; }   // false for system/admin/health-check paths
        bool IsTenantScoped { get; } // false when explicitly operating cross-tenant (operator/system)
    }
}
