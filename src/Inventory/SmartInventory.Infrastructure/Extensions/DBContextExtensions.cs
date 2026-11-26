using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Application.Common.Behaviors;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Identity.Abstractions.Persistence;
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

            services.AddDbContext<SmartInventoryDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("SmartInventoryDb"),
                    sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

            services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<IApplicationDbContext>());
            

            return services;
        }
    }
}
