using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;

namespace SmartInventory.Seeds
{
    public class TenantMembershipSeeder
    {
        private AuthDbContext _authContext;
        public TenantMembershipSeeder(AuthDbContext context)
        {
            _authContext = context;
        }


        public async Task Seed(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File path is not provided or file does not exist. Skipping user seeding.");
                return;
            }

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var tenantsMemberships = JsonConvert.DeserializeObject<List<TenantMembership>>(json) ?? new List<TenantMembership>();

                foreach (var tenantMembership in tenantsMemberships)
                {
                    var exists = await _authContext.TenantMemberships.AnyAsync(t => t.Id == tenantMembership.Id);
                    if (!exists)
                    {
                        _authContext.TenantMemberships.Add(tenantMembership);
                    }
                }

                await _authContext.SaveChangesAsync();

            }
        }
    }
}
