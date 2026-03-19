using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Identity
{
    public class UserRole
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;

        public Role Role { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
