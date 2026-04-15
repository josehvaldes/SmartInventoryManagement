using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Cache
{
    public static class CacheKeys<T>
    {
        public static string ById(Guid id) => $"{typeof(T).Name}:{id}";
    }
}
