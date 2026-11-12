using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Contracts.Responses.Warehouses;
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
    public class GetWarehouseByIdTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private IReadOnlyList<Warehouse> _warehouses;

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
        public async Task GetWarehouseById_ShouldReturnWarehouse()
        {
            var warehouse = _warehouses.First();
            var response = await Client.GetAsync($"/api/v1/warehouses/{warehouse.Id}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var warehouseResponse = await response.Content.ReadFromJsonAsync<WarehouseResponse>(cancellationToken: CancellationToken.None);
            warehouseResponse.Should().NotBeNull();
            warehouseResponse!.Id.Should().Be(warehouse.Id);
            warehouseResponse.Name.Should().Be(warehouse.Name);

        }

        [Fact]
        public async Task GetWarehouseById_ShouldReturnAllWarehouseDetails()
        {
            var warehouse = _warehouses.First();
            var response = await Client.GetAsync($"/api/v1/warehouses/{warehouse.Id}", CancellationToken.None);

            var warehouseResponse = await response.Content.ReadFromJsonAsync<WarehouseResponse>(cancellationToken: CancellationToken.None);

            warehouseResponse.Should().NotBeNull();
            warehouseResponse!.Code.Should().Be(warehouse.Code);
            warehouseResponse.WarehouseType.Should().Be(warehouse.WarehouseType.ToString());
            warehouseResponse.Capacity.Should().Be(warehouse.Capacity);
            warehouseResponse.Street.Should().Be(warehouse.Address?.Street);
            warehouseResponse.City.Should().Be(warehouse.Address?.City);
            warehouseResponse.State.Should().Be(warehouse.Address?.State);
            warehouseResponse.PostalCode.Should().Be(warehouse.Address?.PostalCode);
            warehouseResponse.Country.Should().Be(warehouse.Address?.Country);
        }

        [Fact]
        public async Task GetWarehouseById_ShouldReturnExpectedLinks()
        {
            var warehouse = _warehouses.First();
            var response = await Client.GetAsync($"/api/v1/warehouses/{warehouse.Id}", CancellationToken.None);

            var warehouseResponse = await response.Content.ReadFromJsonAsync<WarehouseResponse>(cancellationToken: CancellationToken.None);

            warehouseResponse.Should().NotBeNull();
            warehouseResponse!.Links.Select(link => link.Rel).Should().BeEquivalentTo("self", "delete-warehouse", "create-warehouse");
            warehouseResponse.Links.Should().OnlyContain(link => !string.IsNullOrEmpty(link.Href));
        }

        [Fact]
        public async Task GetWarehouseById_WhenWarehouseDoesNotExist_ReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/v1/warehouses/{Guid.NewGuid()}", CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetWarehouseById_WithInvalidToken_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/warehouses/{_warehouses.First().Id}");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
