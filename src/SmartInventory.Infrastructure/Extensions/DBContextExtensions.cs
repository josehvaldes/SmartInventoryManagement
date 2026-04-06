using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Infrastructure.Extensions
{
    public static class DBContextExtensions
    {
        public static IServiceCollection AddInventoryDbContext(this IServiceCollection services, IConfiguration config)
        {
            // Expose SmartInventoryDbContext as IApplicationDbContext so handlers can inject it
            services.AddScoped<IApplicationDbContext>(
                provider => provider.GetRequiredService<SmartInventoryDbContext>());

            // Expose AuthDbContext as IAuthDbContext so auth handlers can inject it
            services.AddScoped<IAuthDbContext>(
                provider => provider.GetRequiredService<AuthDbContext>());

            services.AddDbContext<SmartInventoryDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("SmartInventoryDb")));

            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("SmartInventoryDb")));

            return services;
        }
    }
}
