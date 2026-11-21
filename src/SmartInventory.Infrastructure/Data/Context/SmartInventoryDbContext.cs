using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Tenants;
using SmartInventory.Domain.Entities;
using System.Reflection;

namespace SmartInventory.Infrastructure.Data.Context
{
    public class SmartInventoryDbContext : DbContext, IApplicationDbContext
    {
        private readonly IMediator? _mediator;
        private readonly ITenantContext? _tenantContext;
        public interface IInventoryConfiguration { }

        public Guid CurrentTenantId => _tenantContext?.TenantId?? Guid.Empty;

        public bool IsTenantAvailable => _tenantContext?.IsAvailable ?? false;
        public bool IsTenantScoped => _tenantContext?.IsTenantScoped ?? false;
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

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Only configure root entity types; derived types inherit the filter
                if (entityType.BaseType is null &&
                    typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(SmartInventoryDbContext).GetMethod(nameof(ApplyTenantFilter),
                                    BindingFlags.NonPublic | BindingFlags.Instance)!
                                .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(this, new object[] { modelBuilder });
                }
            }

        }

        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, ITenantEntity
        {
            modelBuilder.Entity<TEntity>()
                    .HasQueryFilter(x => IsTenantAvailable && IsTenantScoped && x.TenantId == CurrentTenantId);
        }

        private void EnforceTenantOwnership() 
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                var entity = entry.Entity;
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!IsTenantAvailable)
                            throw new TenantException(TenantErrorCodes.ContextRequired, "No tenant context is available for the entity write.", 401);

                        if (IsTenantScoped)
                            entity.TenantId = CurrentTenantId;
                        
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        if (!IsTenantAvailable || (IsTenantScoped && entity.TenantId != CurrentTenantId))
                        {
                            throw new TenantException(TenantErrorCodes.Forbidden, "tenant overwrite detected.", 403);
                        }
                        break;
                    default: // No action needed for Unchanged or Detached states
                        break;
                }
            }
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnforceTenantOwnership();

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
