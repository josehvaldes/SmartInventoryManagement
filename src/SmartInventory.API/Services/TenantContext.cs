using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.API.Services
{
    public class TenantContext : ITenantContext
    {
        public Guid TenantId => throw new InvalidOperationException("Tenant context is not available.");

        public bool IsAvailable => false;

        public bool IsTenantScoped => false;
    }
}
