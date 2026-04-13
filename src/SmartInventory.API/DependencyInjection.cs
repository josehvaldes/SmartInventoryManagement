using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.API.HealthChecks;
using SmartInventory.API.Settings;
using SmartInventory.API.Validators;
using SmartInventory.Infrastructure.Settings;
using System.Text;
using System.Threading.RateLimiting;

namespace SmartInventory.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAPIDependencies(
            this IServiceCollection services, IConfiguration config)
        {
            services.AddVersioningConfig(config);
            services.AddCustomHealthChecks(config);
            services.AddRateLimiterConfig(config);
            services.AddAuthenticationServices(config);

            services.AddOpenApi("v1");
            services.AddCors(config);
            services.AddCoreFormConfiguration(config);

            services.AddValidatorsFromAssembly(typeof(CreateProductRequestValidator).Assembly);
            return services;
        }

        public static IServiceCollection AddVersioningConfig(
            this IServiceCollection services, IConfiguration config)
        {

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),   // /api/v1/products
                    new HeaderApiVersionReader("X-Api-Version")); // optional fallback
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            return services;
        }

        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services, IConfiguration config)
        {

            var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");

            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException(
                    "JwtSettings:Secret is not configured. " +
                    "Set it via the environment variable: JwtSettings__Secret");
            if (string.IsNullOrWhiteSpace(jwt.Issuer))
                throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured. " +
                    "Set it via the environment variable: JwtSettings__Issuer");
            if (string.IsNullOrWhiteSpace(jwt.Audience))
                throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured. " +
                    "Set it via the environment variable: JwtSettings__Audience");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero  // no tolerance on expiry
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
                .AddPolicy("ManagerOnly", p => p.RequireRole("Admin", "Manager"));
            return services;
        }

        public static IServiceCollection AddCustomHealthChecks(
            this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MemoryCheckOptions>(config.GetSection("MemoryHealthCheck"));
            services.Configure<DiskCheckOptions>(config.GetSection("DiskHealthCheck"));

            services.AddHealthChecks()
                .AddSqlServer(config.GetConnectionString("SmartInventoryDb") ?? string.Empty, tags: new[] { "ready" })
                .AddRedis(config.GetConnectionString("redis") ?? string.Empty, name: "Redis Cache", tags: new[] { "ready" })
                .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "ready" })
                .AddCheck<DiskHealthCheck>("disk", tags: new[] { "ready" })
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

            return services;
        }

        public static IServiceCollection AddRateLimiterConfig(
            this IServiceCollection services, IConfiguration config)
        {

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Global sliding window — general API protection
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,   // splits the window into 6 x 10-second segments
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    });
                });

                // Named policy for write operations (POST/PUT/DELETE)
                options.AddPolicy("WriteOperations", context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    });
                });

                // Stricter policy for auth endpoints
                options.AddPolicy("AuthEndpoints", context =>
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0   // no queuing — reject immediately
                    });
                });

                // Optional: customize the 429 response body
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? (int)retryAfterValue.TotalSeconds
                        : 60;

                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        type = "https://tools.ietf.org/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = 429,
                        retryAfterSeconds = retryAfter
                    }, cancellationToken);
                };
            });

            return services;
        }

        public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration config)
        {
            {
                var corsSettings = config.GetSection("Cors")
                                     .Get<CorsSettings>() ?? new CorsSettings();

                services.AddCors(options =>
                {
                    options.AddPolicy("DefaultCors", policy =>
                    {
                        policy
                            .WithOrigins(corsSettings.AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials(); // Remove this if you don't need cookies/auth headers
                    });

                    // Strict policy for sensitive endpoints
                    options.AddPolicy("StrictCors", policy =>
                    {
                        policy
                            .WithOrigins(corsSettings.AllowedOrigins)
                            .WithHeaders("Content-Type", "Authorization")
                            .WithMethods("GET", "POST", "PUT", "DELETE");
                    });
                });
                return services;
            }
        }

        public static IServiceCollection AddCoreFormConfiguration(this IServiceCollection services, IConfiguration config) 
        {
            var aspnetcoreSettings = config.GetSection("aspnetcore")
                     .Get<AspnetcoreSettings>() ?? new AspnetcoreSettings();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = aspnetcoreSettings.MultipartBodyLengthLimit;
            });
            return services;
        }

    }
}
