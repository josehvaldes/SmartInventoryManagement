using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Identity
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public bool IsActive { get; set; }

    }
}
