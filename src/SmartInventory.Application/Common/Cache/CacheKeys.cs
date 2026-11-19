using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Cache
{
    public static class CacheKeys<T>
    {
        public static string ById(Guid id) => $"{typeof(T).Name}:{id}";
        public static string ByProperty(string name,string property) => $"{typeof(T).Name}:{name}:{property}";

        public static string ByTenantAndId(Guid tenantId, Guid id) => $"{typeof(T).Name}:Tenant:{tenantId}:Id:{id}";
        public static string ByTenantAndProperty(Guid tenantId, string name, string property) => $"{typeof(T).Name}:Tenant:{tenantId}:{name}:{property}";
    }
}
