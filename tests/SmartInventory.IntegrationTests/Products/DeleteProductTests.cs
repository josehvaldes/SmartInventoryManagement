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
    public class DeleteProductTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
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

        [Fact]
        public async Task DeleteProduct_RemovesProductFromDatabase()
        {
            // Arrange
            var productToDelete = ProductData.Products.First();
            // Act
            var response = await Client.DeleteAsync($"/api/v1/products/{productToDelete.Id}", CancellationToken.None);
            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
            // Verify that the product is no longer in the database
            var getResponse = await Client.GetAsync($"/api/v1/products/{productToDelete.Id}", CancellationToken.None);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsNotFound()
        {
            var response = await Client.DeleteAsync($"/api/v1/products/{Guid.NewGuid()}", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidToken_ReturnsUnauthorized()
        {
            var product = ProductData.Products.First();
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/products/{product.Id}");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
