using SmartInventory.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Tenants.DTO
{
    public class TenantMembershipDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public TenantRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }

        public TenantDto Tenant { get; set; } = null!;
    }
}
