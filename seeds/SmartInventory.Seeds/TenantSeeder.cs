
using Newtonsoft.Json;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Seeds.CustomModel;

namespace SmartInventory.Seeds
{
    public class TenantSeeder
    {
        private AuthDbContext _authContext;
        public TenantSeeder(AuthDbContext context)
        {
            _authContext = context;
        }

        public async Task Seed(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File path is not provided or file does not exist. Skipping tenant seeding.");
                return;
            }

            if (_authContext.Tenants.Any())
            {
                return; // Exit if there are already tenants in the database
            }

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var tenants = JsonConvert.DeserializeObject<List<Tenant>>(json) ?? new List<Tenant>();

                if (_authContext.Tenants.Any())
                {
                    return; // Exit if there are already tenants in the database
                }

                foreach (var tenant in tenants)
                {
                    _authContext.Tenants.Add(tenant);
                }

                await _authContext.SaveChangesAsync();

            }
        }
    }
}
