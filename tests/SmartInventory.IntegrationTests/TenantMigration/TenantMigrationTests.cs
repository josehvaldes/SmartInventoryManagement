using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using SmartInventory.Seeds;
using Testcontainers.MsSql;
using Xunit;

namespace SmartInventory.IntegrationTests.TenantMigration
{

    public class TenantMigrationTests()
    {

        [Fact]
        public async Task MigrateLegacyTenant_PersistsLegacyValuesAuthContext()
        {
            await using var database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
            .WithDatabase("SmartInventoryDB")
            .Build();
            var cancellationToken = TestContext.Current.CancellationToken;
            await database.StartAsync(cancellationToken);
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(database.GetConnectionString())
                .Options;

            await using (var context = new AuthDbContext(options))
            {
                await context.Database.MigrateAsync(cancellationToken);

                var tenant = await context.Tenants.Where(t => t.Id == SystemTenants.LegacyTenantId)
                    .SingleAsync(cancellationToken);

                tenant.Id.Should().NotBeEmpty();
                tenant.Id.Should().Be(SystemTenants.LegacyTenantId);

                tenant.Name.Should().Be("Legacy Tenant");
                tenant.Slug.Should().Be("legacy-tenant");
                tenant.Status.Should().Be(TenantStatus.Active);

                var membership = await context.TenantMemberships.SingleAsync(cancellationToken);
                membership.Id.Should().Be(SystemTenants.LegacyTenantMembershipId);
                membership.TenantId.Should().Be(tenant.Id);
                membership.UserId.Should().Be(SystemTenants.LegacyUserId);
                membership.Role.Should().Be(TenantRole.Admin);

                var user = await context.Users.SingleAsync(cancellationToken);
                user.Id.Should().Be(SystemTenants.LegacyUserId);

            }
        }

        [Fact]
        public async Task MigrateLegacyTenant_PersistsLegacyValuesInventoryContext() 
        {
            await using var database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
                .WithDatabase("SmartInventoryDB")
                .Build();
            var cancellationToken = TestContext.Current.CancellationToken;
            await database.StartAsync(cancellationToken);
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(database.GetConnectionString())
                .Options;

            await using (var context = new SmartInventoryDbContext(options)) 
            {
                await context.Database.MigrateAsync(cancellationToken);

                context.Products.Where(p => p.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.PurchaseOrders.Where(po => po.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.PurchaseOrderItems.Where(poi => poi.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.Stocks.Where(s => s.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.StockTransactions.Where(st => st.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.Suppliers.Where(sa => sa.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.Warehouses.Where(w => w.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
                context.StockAlerts.Where(w => w.TenantId != SystemTenants.LegacyTenantId).Should().BeEmpty();
            }
        }
    }
}