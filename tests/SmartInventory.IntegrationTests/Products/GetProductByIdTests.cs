using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Products
{
    [Collection("Integration")]
    public class GetProductByIdTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new SmartInventoryDbContext(options);

            var product = ProductData.Products.First();

            await context.Products.AddAsync(product);

            await context.SaveChangesAsync();
        }

        private async Task<ProductResponse> GetProduct(Guid id)
        {
            var response = await Client.GetAsync($"/api/v1/products/{id}", CancellationToken.None);
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken: CancellationToken.None);
            product.Should().NotBeNull();
            return product!;
        }

        [Fact]
        public async Task GetProductById_ReturnsExpectedProduct()
        {
            // Arrange
            var expectedProduct = ProductData.Products.First();
            // Act
            var product = await GetProduct(expectedProduct.Id);
            // Assert
            product.Should().NotBeNull();
            product.Id.Should().Be(expectedProduct.Id);
            product.Name.Should().Be(expectedProduct.Name);
            product.SKU.Should().Be(expectedProduct.SKU);
            product.Description.Should().Be(expectedProduct.Description);
            product.Category.Should().Be(expectedProduct.Category.ToString());
            product.UnitOfMeasure.Should().Be(expectedProduct.UnitOfMeasure.ToString());
            product.UnitCost.Should().Be(expectedProduct.UnitCost);
            product.IsActive.Should().BeTrue();
            product.Links.Select(link => link.Rel).Should().BeEquivalentTo("self", "delete-product", "create-product");
            product.Links.Should().OnlyContain(link => !string.IsNullOrEmpty(link.Href));
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/v1/products/{Guid.NewGuid()}", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductById_WithInvalidToken_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/products/{ProductData.Products.First().Id}");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
