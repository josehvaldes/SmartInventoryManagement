using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class SmartInventoryDbContext : DbContext, IApplicationDbContext
    {
        public interface IInventoryConfiguration { }

        public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Stock> Stocks => Set<Stock>();

        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

        public DbSet<Warehouse> Warehouses => Set<Warehouse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartInventoryDbContext).Assembly,
                t => t.GetInterfaces().Contains(typeof(IInventoryConfiguration))
            );

        }
    }
}
