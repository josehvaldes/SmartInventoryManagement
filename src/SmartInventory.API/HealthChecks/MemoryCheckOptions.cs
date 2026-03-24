namespace SmartInventory.API.HealthChecks
{
    public class MemoryCheckOptions
    {
        // Triggers Degraded status (default: 768 MB)
        public long DegradedThreshold { get; set; } = 768L * 1024L * 1024L;

        // Triggers Unhealthy status (default: 1 GB)
        public long UnhealthyThreshold { get; set; } = 1024L * 1024L * 1024L;
    }
}
