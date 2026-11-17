using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Entities
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}
