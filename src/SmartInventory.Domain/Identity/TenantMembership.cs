using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;


namespace SmartInventory.Domain.Identity
{
    public class TenantMembership : BaseEntity
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid UserId { get; private set; }
        public TenantRole Role { get; private set; }  // Admin | Manager | Member
        public DateTime JoinedAt { get; private set; }
        public bool IsActive { get; private set; }
    }
}
