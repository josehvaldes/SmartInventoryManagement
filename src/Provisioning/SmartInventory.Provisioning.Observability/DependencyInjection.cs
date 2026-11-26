using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace SmartInventory.Provisioning.Observability
{
    public static class DependencyInjection
    {
        public static void AddPrometheusMapping(this WebApplication app)
        {
            app.UseHttpMetrics();
            app.MapMetrics();

            // Accessing AppMetrics here ensures all custom metrics are registered
            // with the Prometheus registry on startup, before the first scrape.
            _ = AppMetrics.AuthenticationErrors;
        }
    }
}
