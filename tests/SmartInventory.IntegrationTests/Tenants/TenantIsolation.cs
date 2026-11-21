using FluentAssertions;
using Microsoft.Data.SqlClient;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Identity;
using SmartInventory.IntegrationTests.Data;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SmartInventory.IntegrationTests.Tenants
{
    [Collection("Integration")]
    public class TenantIsolation(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private readonly List<Product> _products = ProductData.LoadProductsFromFile();
        private readonly List<Tenant> _tenants = TenantData.LoadTenantsFromFile();

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            using var context = CreateSmartInventoryDbContext(false);
            using var authContext = CreateAuthDbContext();
            
            foreach (var tenant in _tenants)
            {
                if (!await authContext.Tenants.AnyAsync(t => t.Id == tenant.Id))
                    authContext.Tenants.Add(tenant);
            }

            foreach (var product in _products)
            {
                //products are saved with its default tenant id, which is not the same as the legacy tenant id.
                context.Products.Add(product);
            }

            await authContext.SaveChangesAsync();
            await context.SaveChangesAsync();
            
        }

        [Fact]
        public async Task GetProducts_WithDifferentTenantId_ReturnsEmpty()
        {
            var query = "pageNumber=1&pageSize=10";

            var response = await Client.GetAsync($"/api/v1/products?{query}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var page = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>(cancellationToken: CancellationToken.None);
            var items = page!.Items;
            items.Should().NotBeNull();
            
            // should be empty because the products are seeded with a different tenant id than the legacy tenant id, and the request is made with the legacy tenant id.
            items.Should().HaveCount(0);
        }

        [Fact]
        public async Task DeleteProduct_WithDifferentTenantId_ReturnsNotFound()
        {
            var firstProduct = _products.First();

            var response = await Client.DeleteAsync($"/api/v1/products/{firstProduct.Id}", CancellationToken.None);
            // should return not found because the product is seeded with a different tenant id than the legacy tenant id, and the request is made with the legacy tenant id.
            // the product is not found because the tenant id does not match.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
