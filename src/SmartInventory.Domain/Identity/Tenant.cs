using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;


namespace SmartInventory.Domain.Identity
{
    public class Tenant: BaseEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;          // display name, max 200
        public string Slug { get; private set; } = string.Empty;          // URL-safe Identifier, globally unique, max 50
        public TenantStatus Status { get; private set; } // Active | Suspended | Provisioning
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
    }
}
