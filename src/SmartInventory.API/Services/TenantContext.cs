using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.API.Services
{
    public class TenantContext : ITenantContext
    {
        public Guid TenantId { get; private set; } = SystemTenants.LegacyTenantId;

        public bool IsAvailable { get; private set; } = true;

        public bool IsTenantScoped { get; private set; } = true;
    }
}
