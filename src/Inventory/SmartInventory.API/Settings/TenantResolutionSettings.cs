namespace SmartInventory.API.Settings
{
    public class TenantResolutionSettings
    {
        public const string SectionName = "TenantResolution";
        public string BaseDomain { get; set; } = "localhost";
        public string SlugHeaderName { get; set; } = "X-Tenant-Slug";
    }
}
