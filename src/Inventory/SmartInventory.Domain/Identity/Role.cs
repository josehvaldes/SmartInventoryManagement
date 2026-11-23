using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Identity
{
    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
