using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Warehouses
{
    [Collection("Integration")]
    public class DeleteWarehouseTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private List<Warehouse> _warehouses;
        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new SmartInventoryDbContext(options);

            _warehouses = WarehouseData.LoadWarehousesFromFile();
            var first = _warehouses.First();
            await context.Warehouses.AddAsync(first);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteWarehouse_ShouldReturnNoContent()
        {
            var warehouseToDelete = _warehouses.First();
            var response = await Client.DeleteAsync($"/api/v1/warehouses/{warehouseToDelete.Id}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            await using var context = new SmartInventoryDbContext(options);
            context.Warehouses.Should().NotContain(w => w.Id == warehouseToDelete.Id);
        }

        [Fact]
        public async Task DeleteWarehouse_RemovesWarehouse_AndSubsequentGetReturnsNotFound()
        {
            var warehouseToDelete = _warehouses.First();

            var deleteResponse = await Client.DeleteAsync($"/api/v1/warehouses/{warehouseToDelete.Id}", CancellationToken.None);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await Client.GetAsync($"/api/v1/warehouses/{warehouseToDelete.Id}", CancellationToken.None);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteWarehouse_WhenWarehouseDoesNotExist_ReturnsNotFound()
        {
            var response = await Client.DeleteAsync($"/api/v1/warehouses/{Guid.NewGuid()}", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteWarehouse_WithInvalidToken_ReturnsUnauthorized()
        {
            var warehouse = _warehouses.First();
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/warehouses/{warehouse.Id}");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
