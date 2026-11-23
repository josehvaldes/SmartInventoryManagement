using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartInventory.API.HealthChecks
{
    public class DiskHealthCheck(IOptions<DiskCheckOptions> options) : IHealthCheck
    {
        private readonly DiskCheckOptions _options = options.Value;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var drive = new DriveInfo(_options.DriveName);
            var freeSpacePercentage = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;

            var data = new Dictionary<string, object>()
                {
                    { "DriveName", drive.Name },
                    { "TotalSizeGigaBytes", Math.Round(drive.TotalSize/(Math.Pow(1024,3)), 2)  },
                    { "AvailableFreeSpaceGigaBytes", Math.Round(drive.AvailableFreeSpace/(Math.Pow(1024,3)), 2) },
                    { "FreeSpacePercentage", Math.Round(freeSpacePercentage, 2) }
                };

            HealthStatus status;
            string description;

            if (freeSpacePercentage < _options.UnhealthyThresholdPercentage)
            {
                status = HealthStatus.Unhealthy;
                description = $"Disk space critically low: {freeSpacePercentage:F2}% free (threshold: {_options.UnhealthyThresholdPercentage}%).";
            }
            else if (freeSpacePercentage < _options.DegradedThresholdPercentage) 
            {
                status = HealthStatus.Degraded;
                description = $"Disk space low: {freeSpacePercentage:F2}% free (warning at: {_options.DegradedThresholdPercentage}%).";
            }
            else
            {
                status = HealthStatus.Healthy;
                description = $"Disk space sufficient: {freeSpacePercentage:F2}% free.";
            }


            return Task.FromResult(new HealthCheckResult(status, description, data: data));
        }
    }
}
