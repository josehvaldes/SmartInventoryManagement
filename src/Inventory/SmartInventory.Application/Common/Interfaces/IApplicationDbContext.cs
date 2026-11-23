using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Behaviors;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IApplicationDbContext: IUnitOfWork
    {
        DbSet<Product> Products { get; }
        DbSet<Stock> Stocks { get; }
        DbSet<PurchaseOrder> PurchaseOrders{ get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
        DbSet<Warehouse> Warehouses { get; }
    }
}
