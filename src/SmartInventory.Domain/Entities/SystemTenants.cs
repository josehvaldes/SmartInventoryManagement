using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    public static class SystemTenants
    {
        public static readonly Guid LegacyTenantId = Guid.Parse("99999999-0000-0000-0000-000000000100");
        public static readonly Guid LegacyTenantMembershipId = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000001");
        public static readonly Guid LegacyUserId = Guid.Parse("44444444-0000-0000-0000-000000000100");

    }
}
