using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GarnetCacheService> _logger;

        public GarnetCacheService(IConnectionMultiplexer mux, IConfiguration config, ILogger<GarnetCacheService> logger)
        {
            _db = mux.GetDatabase();
            _defaultTtlSeconds = int.Parse(config["Cache:DefaultTTLSeconds"]!);
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);

                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>((string)value!);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for key '{Key}'. Falling through to source.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);

                await _db.StringSetAsync(
                    key,
                    json,
                    ttl ?? TimeSpan.FromSeconds(_defaultTtlSeconds)
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for key '{Key}'. Continuing without cache.", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache remove failed for key '{Key}'. Continuing without cache.", key);
            }
        }
    }
}
