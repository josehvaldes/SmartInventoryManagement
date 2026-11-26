using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Identity.Abstractions.Persistence;
using SmartInventory.Identity.Infrastructure.Data.Context;

namespace SmartInventory.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Expose AuthDbContext as IAuthDbContext so auth handlers can inject it
            services.AddScoped<IAuthDbContext>(
                provider => provider.GetRequiredService<AuthDbContext>());
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SmartInventoryDb"),
                    sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));
            services.AddScoped<IAuthUnitOfWork>(p => p.GetRequiredService<IAuthDbContext>());
            return services;
        }
    }
}
