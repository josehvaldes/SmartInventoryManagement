using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    public class SmartInventoryWebApplicationFactory(string connectionString, string redisConnectionString): WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                var inventoryOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SmartInventoryDbContext>));
                if (inventoryOptionsDescriptor is not null)
                {
                    services.Remove(inventoryOptionsDescriptor);
                }

                var authOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                if (authOptionsDescriptor is not null)
                {
                    services.Remove(authOptionsDescriptor);
                }

                // Add DbContexts using the Testcontainers connection string
                services.AddDbContext<SmartInventoryDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });

                services.AddDbContext<AuthDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });
            });





            builder.UseEnvironment("Testing");

            // appsettings.json leaves the local JWT settings empty and Program.cs validates them eagerly,
            // before ConfigureAppConfiguration sources are applied, so they must be host settings.
            builder.UseSetting("JwtSettings:Secret", "test-secret-key-long-enough-for-hmac-32+chars");
            builder.UseSetting("JwtSettings:Issuer", "test-issuer");
            builder.UseSetting("JwtSettings:Audience", "test-audience");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SmartInventoryDb"] = connectionString,
                    ["ConnectionStrings:redis"] = redisConnectionString
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
            });
        }
    }
}
