using Microsoft.Extensions.Configuration;
using SmartInventory.Application.Common.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SmartInventory.Infrastructure.Data.Cache
{
    public class GarnetCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly int _defaultTtlSeconds;

        public GarnetCacheService(IConnectionMultiplexer mux, IConfiguration config)
        {
            _db = mux.GetDatabase();
            _defaultTtlSeconds = int.Parse(config["Cache:DefaultTTLSeconds"]!);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>((string)value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var json = JsonSerializer.Serialize(value);

            await _db.StringSetAsync(
                key,
                json,
                ttl ?? TimeSpan.FromSeconds(_defaultTtlSeconds)
            );
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}
