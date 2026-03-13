using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<Stock> Stocks { get; }
        DbSet<PurchaseOrder> PurchaseOrders{ get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    }
}
