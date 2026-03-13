using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class SmartInventoryDbContext : DbContext, IApplicationDbContext
    {
        public interface IInventoryConfiguration { }

        public SmartInventoryDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Stock> Stocks => Set<Stock>();

        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartInventoryDbContext).Assembly,
                t => t.GetInterfaces().Contains(typeof(IInventoryConfiguration))
            );

        }
    }
}
