namespace SmartInventory.API.HealthChecks
{
    public class DiskCheckOptions
    {
        public string DriveName { get; set; } = "C";
        public long DegradedThresholdPercentage { get; set; } = 20;
        public long UnhealthyThresholdPercentage { get; set; } = 10;
    }
}
