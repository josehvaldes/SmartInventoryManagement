using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Infrastructure.Settings;
using System.Text;

namespace SmartInventory.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAPIDependencies(
            this IServiceCollection services,
            IConfiguration config)
        {
            var jwt = config.GetSection("JwtSettings").Get<JwtSettings>()!;

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
    }
}
