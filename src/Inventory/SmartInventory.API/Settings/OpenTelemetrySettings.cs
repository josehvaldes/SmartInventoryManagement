namespace SmartInventory.API.Settings
{
    public sealed class OpenTelemetrySettings
    {
        public const string SectionName = "OpenTelemetry";

        public string OtlpEndpoint { get; set; } = "localhost:4317";

    }
}
