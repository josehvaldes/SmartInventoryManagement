using Newtonsoft.Json;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (_authContext.TenantMemberships.Any())
            {
                return; // Exit if there are already tenants in the database
            }

            using (var reader = new StreamReader(filePath))
            {
                var json = await reader.ReadToEndAsync();
                var tenantsMemberships = JsonConvert.DeserializeObject<List<TenantMembership>>(json) ?? new List<TenantMembership>();

                if (_authContext.TenantMemberships.Any())
                {
                    return; // Exit if there are already tenants in the database
                }

                foreach (var tenantMembership in tenantsMemberships)
                {
                    _authContext.TenantMemberships.Add(tenantMembership);
                }

                await _authContext.SaveChangesAsync();

            }
        }
    }
}
