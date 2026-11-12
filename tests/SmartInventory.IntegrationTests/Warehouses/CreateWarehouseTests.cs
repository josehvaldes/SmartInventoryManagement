using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Requests.Warehouses;
using SmartInventory.Infrastructure.Data.Context;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SmartInventory.IntegrationTests.Warehouses
{
    [Collection("Integration")]
    public class CreateWarehouseTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {

        protected override async Task SeedDataAsync(SqlConnection connection)
        {

        }

        private static CreateWarehouseRequest BuildValidRequest(SmartInventory.Domain.Entities.Warehouse warehouse) => new()
        {
            Name = warehouse.Name,
            Code = warehouse.Code,
            Capacity = warehouse.Capacity,

            City = warehouse.Address?.City,
            State = warehouse.Address?.State,
            Street = warehouse.Address?.Street,
            Country = warehouse.Address?.Country,
            PostalCode = warehouse.Address?.PostalCode,

            WarehouseType = warehouse.WarehouseType.ToString(),
            ManagerEmail = warehouse.ManagerEmail,
            ManagerName = warehouse.ManagerName,
            ManagerPhone = warehouse.ManagerPhone,
        };

        [Fact]
        public async Task CreateWarehouse_ShouldReturnSuccess()
        {
            var warehouse = WarehouseData.LoadWarehousesFromFile().First();

            var createWarehouseRequest = new CreateWarehouseRequest {
                Name = warehouse.Name,
                Code = warehouse.Code,
                Capacity = warehouse.Capacity,

                City = warehouse.Address?.City,
                State = warehouse.Address?.State,
                Street = warehouse.Address?.Street,
                Country = warehouse.Address?.Country,
                PostalCode = warehouse.Address?.PostalCode,

                WarehouseType = warehouse.WarehouseType.ToString(),
                ManagerEmail = warehouse.ManagerEmail,
                ManagerName = warehouse.ManagerName,
                ManagerPhone = warehouse.ManagerPhone,
            };

            var response = await Client.PostAsJsonAsync("/api/v1/warehouses", createWarehouseRequest, CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            await using var context = new SmartInventoryDbContext(options);
            var createdWarehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouse.Name && w.Capacity == warehouse.Capacity, CancellationToken.None);
            createdWarehouse.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateWarehouse_ShouldReturnExpectedLinks()
        {
            var warehouse = WarehouseData.LoadWarehousesFromFile().First();
            var createWarehouseRequest = BuildValidRequest(warehouse);

            var response = await Client.PostAsJsonAsync("/api/v1/warehouses", createWarehouseRequest, CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var root = document!.RootElement;
            var id = root.GetProperty("id").GetGuid();
            id.Should().NotBeEmpty();

            var links = root.GetProperty("links").EnumerateArray().ToArray();
            links.Select(link => link.GetProperty("rel").GetString())
                .Should()
                .BeEquivalentTo("self", "delete-warehouse", "create-warehouse");
            links.Should().OnlyContain(link => !string.IsNullOrEmpty(link.GetProperty("href").GetString()));
        }

        [Fact]
        public async Task CreateWarehouse_WithMissingRequiredFields_ReturnsUnprocessableEntity()
        {
            var response = await Client.PostAsJsonAsync("/api/v1/warehouses", new CreateWarehouseRequest(), CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            var errors = document!.RootElement.GetProperty("errors");

            errors.TryGetProperty("Code", out _).Should().BeTrue();
            errors.TryGetProperty("Name", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateWarehouse_WithInvalidManagerEmail_ReturnsUnprocessableEntity()
        {
            var warehouse = WarehouseData.LoadWarehousesFromFile().First();
            var createWarehouseRequest = BuildValidRequest(warehouse);
            createWarehouseRequest.ManagerEmail = "not-a-valid-email";

            var response = await Client.PostAsJsonAsync("/api/v1/warehouses", createWarehouseRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors").TryGetProperty("ManagerEmail", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateWarehouse_WithDuplicateCode_ReturnsUnprocessableEntity()
        {
            var warehouse = WarehouseData.LoadWarehousesFromFile().First();
            var createWarehouseRequest = BuildValidRequest(warehouse);

            var firstResponse = await Client.PostAsJsonAsync("/api/v1/warehouses", createWarehouseRequest, CancellationToken.None);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var duplicateRequest = BuildValidRequest(warehouse);
            duplicateRequest.Name = "A different warehouse name";
            var duplicateResponse = await Client.PostAsJsonAsync("/api/v1/warehouses", duplicateRequest, CancellationToken.None);

            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            using var document = await duplicateResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: CancellationToken.None);
            document!.RootElement.GetProperty("errors").TryGetProperty("Code", out _).Should().BeTrue();
        }

        [Fact]
        public async Task CreateWarehouse_WithInvalidToken_ReturnsUnauthorized()
        {
            var warehouse = WarehouseData.LoadWarehousesFromFile().First();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/warehouses")
            {
                Content = JsonContent.Create(BuildValidRequest(warehouse))
            };
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
