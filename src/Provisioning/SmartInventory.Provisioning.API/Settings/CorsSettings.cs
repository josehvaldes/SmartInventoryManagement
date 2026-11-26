namespace SmartInventory.Provisioning.API.Settings
{
    public sealed class CorsSettings
    {
        public const string SectionName = "Cors";
        public string[] AllowedOrigins { get; init; } = [];
    }
}
