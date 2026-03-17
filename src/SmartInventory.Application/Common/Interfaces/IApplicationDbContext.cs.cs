using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<Stock> Stocks { get; }
        DbSet<PurchaseOrder> PurchaseOrders{ get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
