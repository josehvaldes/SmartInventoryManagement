using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Domain.Enums
{
    /// <summary>
    /// Represents the various categories of products in the inventory system.
    /// </summary>
    public enum ProductCategory
    {
        Electronics = 1,
        Consumables = 2,
        Equipment = 3,
        Tools = 4,
        Safety = 5,
        RawMaterials = 6,
        FinishedGoods = 7,
        Packaging = 8,
        Other = 99
    }
}
