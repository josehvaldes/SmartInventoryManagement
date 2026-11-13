namespace SmartInventory.API.Settings
{
    public sealed class OpenTelemetrySettings
    {
        public static readonly string SectionName = "OpenTelemetry";

        public string OtlpEndpoint { get; set; } = "localhost:4317";

    }
}
