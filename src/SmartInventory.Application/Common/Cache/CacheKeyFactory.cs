using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Common.Cache
{
    public sealed class CacheKeyFactory(ITenantContext tenantContext) : ICacheKeyFactory
    {
        public string ById<T>(Guid id)
        {
            return $"{typeof(T).Name}:Tenant:{tenantContext.TenantId}:Id:{id}";
        }

        public string ByProperty<T>(string name, string property)
        {
            return $"{typeof(T).Name}:Tenant:{tenantContext.TenantId}:{name}:{property}";
        }
    }
}
