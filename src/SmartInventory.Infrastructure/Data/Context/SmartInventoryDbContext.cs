using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class SmartInventoryDbContext : DbContext, IApplicationDbContext
    {
        private readonly IMediator? _mediator;
        private readonly ITenantContext? _tenantContext;
        public interface IInventoryConfiguration { }

        public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options) : base(options)
        {
        }

        public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options, ITenantContext tenantContext, IMediator? mediator = null) : base(options)
        {
            _tenantContext = tenantContext;
            _mediator = mediator;
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Stock> Stocks => Set<Stock>();

        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

        public DbSet<Warehouse> Warehouses => Set<Warehouse>();

        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<StockAlert> StockAlerts => Set<StockAlert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartInventoryDbContext).Assembly,
                t => t.GetInterfaces().Contains(typeof(IInventoryConfiguration))
            );
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var needsTenant = ChangeTracker
                .Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty)
                .ToList();

            if (needsTenant.Count > 0)
            {
                if (_tenantContext is not { IsAvailable: true })
                    throw new InvalidOperationException("A tenant is required to save tenant entities, but no tenant context is available.");

                needsTenant.ForEach(e => e.Entity.TenantId = _tenantContext.TenantId);
            }

            var domainEventEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .ToList();

            var domainEvents = domainEventEntities
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            domainEventEntities.ForEach(e => e.Entity.ClearDomainEvents());

            var result = await base.SaveChangesAsync(cancellationToken);

            if (_mediator is not null)
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
            }

            return result;
        }
    }
}
