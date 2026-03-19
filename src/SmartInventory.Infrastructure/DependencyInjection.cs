using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Identity;
using SmartInventory.Domain.Interfaces;
using SmartInventory.Infrastructure.Auth;
using SmartInventory.Infrastructure.Data.Cache;
using SmartInventory.Infrastructure.Data.Context;
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

            // Expose SmartInventoryDbContext as IApplicationDbContext so handlers can inject it
            services.AddScoped<IApplicationDbContext>(
                provider => provider.GetRequiredService<SmartInventoryDbContext>());

            // Expose AuthDbContext as IAuthDbContext so auth handlers can inject it
            services.AddScoped<IAuthDbContext>(
                provider => provider.GetRequiredService<AuthDbContext>());

            services.AddDbContext<SmartInventoryDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddSingleton<ICacheService, GarnetCacheService>();

            
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var connectionString = config["Cache:ConnectionString"] ?? string.Empty;
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
