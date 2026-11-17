
using Microsoft.EntityFrameworkCore;
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

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var tenants = JsonConvert.DeserializeObject<List<Tenant>>(json) ?? new List<Tenant>();


                foreach (var tenant in tenants)
                {
                    var exists = await _authContext.Tenants.AnyAsync(t => t.Id == tenant.Id);
                    if (!exists)
                    {
                        _authContext.Tenants.Add(tenant);
                    }
                }

                await _authContext.SaveChangesAsync();

            }
        }
    }
}
