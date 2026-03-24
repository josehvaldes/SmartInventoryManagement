using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace SmartInventory.API
{
    public static class ApplicationMapping
    {

        public static void MapCustomBehaviors(this WebApplication app) 
        {
            //Map HealthCheck to root for easy access

            app.MapHealthChecks("/health").AllowAnonymous();

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse  // optional, richer JSON
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });

            //app.MapOpenApi(); // Serves the spec at /openapi/v1.json
            if (app.Environment.IsDevelopment())
            {
                // Serve each version at its own URL
                app.MapOpenApi("/openapi/{documentName}.json");
            }
            app.MapScalarApiReference(); // Scalar UI (requires Scalar.AspNetCore package)

        }
    }
}
