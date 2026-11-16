using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.Seeds.CustomModel;
using System.Reflection;

namespace SmartInventory.Seeds
{

    public class TenantMigrationSeeder
    {
        private readonly AuthDbContext _authContext;
        private PasswordHasher<User> _hasher;

        public TenantMigrationSeeder(AuthDbContext context)
        {
            _authContext = context;
            _hasher = new PasswordHasher<User>();
        }

        public async Task MigrateLegacyTenant(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A legacy tenant file path is required.", nameof(filePath));

            var json = await File.ReadAllTextAsync(filePath);
            var legacy = JObject.Parse(json);
            
            var tenantNode = legacy["tenant"] as JObject ?? throw new InvalidOperationException("The legacy file must contain a tenant object.");
            var membershipNode = legacy["membership"] as JObject ?? throw new InvalidOperationException("The legacy file must contain a membership object.");

            var serializer = JsonSerializer.Create(SeederSettings.DeserializationSettings);

            var tenant = tenantNode.ToObject<Tenant>(serializer)
                ?? throw new InvalidOperationException("Failed to deserialize tenant.");
            var membership = membershipNode.ToObject<TenantMembership>(serializer)
                ?? throw new InvalidOperationException("Failed to deserialize tenant membership.");
            
            var userNode = legacy["user"] as JObject ?? throw new InvalidOperationException("The legacy file must contain a user object.");
            var user = userNode.ToObject<CustomUser>(serializer)
                ?? throw new InvalidOperationException("Failed to deserialize user.");

            if (tenant.Id == Guid.Empty || membership.Id == Guid.Empty || membership.UserId == Guid.Empty || membership.TenantId != tenant.Id 
                || user.Id == Guid.Empty || membership.UserId != user.Id)
                throw new InvalidOperationException("The legacy tenant and membership must have valid matching identifiers.");

            user.PasswordHash = _hasher.HashPassword(user, user.Password);

            if (!_authContext.Tenants.Any(t => t.Id == tenant.Id))
                _authContext.Tenants.Add(tenant);
            if (!_authContext.TenantMemberships.Any(m => m.Id == membership.Id))
                _authContext.TenantMemberships.Add(membership);
            if (!_authContext.Users.Any(u => u.Id == user.Id))
                _authContext.Users.Add(user);

            await _authContext.SaveChangesAsync();
        }

        private sealed class PrivateSetterContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (!property.Writable && member is PropertyInfo propertyInfo && propertyInfo.GetSetMethod(nonPublic: true) != null)
                    property.Writable = true;
                return property;
            }
        }
    }
}
