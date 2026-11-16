using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using SmartInventory.Seeds;
using Testcontainers.MsSql;
using Xunit;

namespace SmartInventory.IntegrationTests.TenantMigration
{

    [Collection("Integration")]
    public class TenantMigrationTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(SqlConnection connection)
        {

        }

        [Fact]
        public async Task MigrateLegacyTenant_PersistsLegacyValues()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using (var context = new AuthDbContext(options))
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "legacy_tenant.json");
                await new TenantMigrationSeeder(context).MigrateLegacyTenant(filePath);
            }

            await using (var context = new AuthDbContext(options))
            {
                var tenant = await context.Tenants.SingleAsync(cancellationToken);
                tenant.Id.Should().NotBeEmpty();
                tenant.Id.Should().Be(Guid.Parse("99999999-0000-0000-0000-000000000100"));

                tenant.Name.Should().Be("Legacy Tenant");
                tenant.Slug.Should().Be("legacy-tenant");
                tenant.Status.Should().Be(TenantStatus.Active);
                tenant.CreatedAt.Should().Be(new DateTime(2026, 11, 15, 8, 0, 0));
                tenant.UpdatedAt.Should().Be(tenant.CreatedAt);

                var membership = await context.TenantMemberships.SingleAsync(cancellationToken);
                membership.Id.Should().Be(Guid.Parse("AAAAAAAA-0000-0000-0000-000000000001"));
                membership.TenantId.Should().Be(tenant.Id);
                membership.UserId.Should().Be(Guid.Parse("44444444-0000-0000-0000-000000000100"));
                membership.Role.Should().Be(TenantRole.Admin);
                membership.JoinedAt.Should().Be(tenant.CreatedAt);
                membership.IsActive.Should().BeTrue();

                var user = await context.Users.SingleAsync(cancellationToken);
                user.Id.Should().Be(Guid.Parse("44444444-0000-0000-0000-000000000100"));

            }
        }
    }
}