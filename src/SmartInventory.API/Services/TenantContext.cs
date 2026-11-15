using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.API.Services
{
    public class TenantContext : ITenantContext
    {
        public Guid TenantId => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsTenantScoped => throw new NotImplementedException();
    }
}
