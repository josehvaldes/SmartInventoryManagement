using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Auth;
using SmartInventory.Infrastructure.Data.Cache;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Infrastructure.Extensions;
using SmartInventory.Infrastructure.Settings;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration config)
        {
            // Register DbContexts and expose them as interfaces for handlers to inject
            services.AddInventoryDbContext(config);

            // Register Quartz jobs
            services.AddQuartzJobs(config);

            services.AddSingleton<ICacheService, GarnetCacheService>();
                        
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("redis") ?? string.Empty;
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
