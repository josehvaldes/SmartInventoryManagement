using Newtonsoft.Json;
using SmartInventory.Domain.Identity;


namespace SmartInventory.IntegrationTests.Common
{
    public static class TenantData
    {
        private static readonly string TenantsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "tenants.json");
        private static readonly string TenantMembershipsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "tenant_memberships.json");

        private static readonly JsonSerializerSettings _deserializationSettings = new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static List<Tenant> LoadTenantsFromFile()
        {
            if (string.IsNullOrWhiteSpace(TenantsFilePath) || !File.Exists(TenantsFilePath))
                throw new FileNotFoundException("File path is not provided or file does not exist. Skipping tenant seeding.");
            using (var reader = new StreamReader(TenantsFilePath))
            {
                var tenants = new List<Tenant>();
                var json = reader.ReadToEnd();
                var seeds = JsonConvert.DeserializeObject<List<Tenant>>(json, _deserializationSettings) ?? new List<Tenant>();
                tenants.AddRange(seeds);
                return tenants;
            }
        }

        public static List<TenantMembership> LoadTenantMembershipsFromFile()
        {
            if (string.IsNullOrWhiteSpace(TenantMembershipsFilePath) || !File.Exists(TenantMembershipsFilePath))
                throw new FileNotFoundException("File path is not provided or file does not exist. Skipping tenant membership seeding.");
            using (var reader = new StreamReader(TenantMembershipsFilePath))
            {
                var memberships = new List<TenantMembership>();
                var json = reader.ReadToEnd();
                var seeds = JsonConvert.DeserializeObject<List<TenantMembership>>(json, _deserializationSettings) ?? new List<TenantMembership>();
                memberships.AddRange(seeds);
                return memberships;
            }
        }
    }
}
