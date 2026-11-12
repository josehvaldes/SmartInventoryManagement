using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.IntegrationTests.Infrastructure
{
    public class SmartInventoryWebApplicationFactory(string connectionString, string redisConnectionString, string s3ServiceUrl, string s3BucketName): WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Point the S3 client at LocalStack instead of real AWS
                services.RemoveAll<IAmazonS3>();
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                    new BasicAWSCredentials("test", "test"),
                    new AmazonS3Config
                    {
                        ServiceURL = s3ServiceUrl,
                        AuthenticationRegion = "us-east-1",
                        ForcePathStyle = true,
                        RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                        ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
                    }));

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
            builder.UseSetting("AwsSettings:S3BucketName", s3BucketName);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SmartInventoryDb"] = connectionString,
                    ["ConnectionStrings:redis"] = redisConnectionString,
                    ["RateLimiting:AuthEndpoints:PermitLimit"] = "1000"
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
