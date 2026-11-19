using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.API.Services
{
    public class TenantContext : ITenantContext
    { 

        public TenantContext() { }

        public Guid ResolvedTenantId { get; private set; } = Guid.Empty;
        public Guid TenantId { get; private set; } = Guid.Empty;

        public bool IsAvailable { get; private set; } = false;

        public bool IsTenantScoped { get; private set; } = false;

        public string Role { get; private set; } = string.Empty;

        public void SetResolvedTenantId(Guid tenantId) 
        {
            ResolvedTenantId = tenantId;
            IsAvailable = false;
        }

        public void Initialize(Guid tenantId, string role, bool isTenantScoped = true)
        {
            TenantId = tenantId;
            Role = role;
            IsAvailable = true;
            IsTenantScoped = isTenantScoped;
        }
    }
}
