using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Identity;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Common;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.TenantResolution
{
    [Collection("Integration")]
    public class TestResolutionTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private List<Tenant> tenants = null!;
        private List<TenantMembership> memberships = null!;
        private List<User> users = null!;

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            await using var context = new AuthDbContext(options);
            users = UserData.LoadUsersFromFile();
            foreach (var user in users)
            {
                if (!await context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    await context.Users.AddAsync(user);
                }
            }

            tenants = TenantData.LoadTenantsFromFile();
            foreach (var tenant in tenants)
            {
                if (!await context.Tenants.AnyAsync(t => t.Id == tenant.Id))
                {
                    await context.Tenants.AddAsync(tenant);
                }
            }
            memberships = TenantData.LoadTenantMembershipsFromFile();
            foreach (var membership in memberships)
            {
                if (!await context.TenantMemberships.AnyAsync(tm => tm.Id == membership.Id))
                {
                    await context.TenantMemberships.AddAsync(membership);
                }
            }

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task TenantResolutionMiddleware_ShouldResolveTenantFromHeader()
        {
            using var response = await GetWarehousesAsync("legacy-tenant");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task TenantResolutionMiddleware_ShouldReturnNotFoundForUnknownTenant()
        {
            using var response = await GetWarehousesAsync("unknown");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task TenantResolutionMiddleware_ShouldRejectInvalidTenantSlug()
        {
            using var response = await GetWarehousesAsync("invalid slug");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task TenantResolutionMiddleware_ShouldRejectInactiveTenant()
        {
            using var response = await GetWarehousesAsync("globex");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task TenantContextMiddleware_ShouldRejectTenantSlugThatDoesNotMatchAuthenticatedTenant()
        {
            using var response = await GetWarehousesAsync("acme");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task TenantContextMiddleware_ShouldUseAuthenticatedTenantWhenNoSlugIsProvided()
        {
            using var response = await GetWarehousesAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task TenantContextMiddleware_ShouldRejectUserWithoutTenantMembership()
        {
            using var response = await GetWarehousesAsync(authorizationToken: TestAuthHandler.NoMembershipToken);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private async Task<HttpResponseMessage> GetWarehousesAsync(string? tenantSlug = null, string? authorizationToken = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/warehouses?pageNumber=1&pageSize=10");
            if (tenantSlug is not null)
            {
                request.Headers.Add("X-Tenant-Slug", tenantSlug);
            }
            if (authorizationToken is not null)
            {
                request.Headers.Add("Authorization", $"Bearer {authorizationToken}");
            }

            return await Client.SendAsync(request, CancellationToken.None);
        }
    }
}
