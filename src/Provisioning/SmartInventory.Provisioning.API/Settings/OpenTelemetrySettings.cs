namespace SmartInventory.Provisioning.API.Settings
{
    public class OpenTelemetrySettings
    {
        public const string SectionName = "OpenTelemetry";
        public string OtlpEndpoint { get; set; } = "otel-collector:4317";
    }
}
