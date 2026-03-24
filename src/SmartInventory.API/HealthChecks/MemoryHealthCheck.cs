using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace SmartInventory.API.HealthChecks
{
    public class MemoryHealthCheck(IOptions<MemoryCheckOptions> options) : IHealthCheck
    {
        private readonly MemoryCheckOptions _options = options.Value;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var data = new Dictionary<string, object>()
                {
                    { "AllocatedBytes", allocated },
                    { "Gen0Collections", GC.CollectionCount(0) },
                    { "Gen1Collections", GC.CollectionCount(1) },
                    { "Gen2Collections", GC.CollectionCount(2) },
                };

            HealthStatus status;
            string description;

            if (allocated >= _options.UnhealthyThreshold)
            {
                status = HealthStatus.Unhealthy;
                description = $"Memory usage is critical: {allocated:N0} bytes allocated (limit: {_options.UnhealthyThreshold:N0} bytes).";
            }
            else if (allocated >= _options.DegradedThreshold)
            {
                status = HealthStatus.Degraded;
                description = $"Memory usage is elevated: {allocated:N0} bytes allocated (warning at: {_options.DegradedThreshold:N0} bytes).";
            }
            else
            {
                status = HealthStatus.Healthy;
                description = $"Memory usage is normal: {allocated:N0} bytes allocated.";
            }

            return Task.FromResult(new HealthCheckResult(status, description, data: data));
        }
    }
}
