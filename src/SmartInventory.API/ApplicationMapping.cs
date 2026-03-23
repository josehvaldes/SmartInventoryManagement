using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

        }
    }
}
