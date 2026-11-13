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
    public class GetWarehousesTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private IReadOnlyList<Warehouse> _warehouses = null!;

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var options = new DbContextOptionsBuilder<SmartInventoryDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new SmartInventoryDbContext(options);

            _warehouses = WarehouseData.LoadWarehousesFromFile();
            await context.Warehouses.AddRangeAsync(_warehouses);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetWarehouses_ShouldReturnExpectedWarehouses()
        {
            // Arrange
            var query = "pageNumber=1&pageSize=10";
            // Act
            var response = await Client.GetAsync($"/api/v1/warehouses?{query}", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var page = await response.Content.ReadFromJsonAsync<PagedResponse<WarehouseResponse>>(cancellationToken: CancellationToken.None);
            page.Should().NotBeNull();
            // Assert
            page!.Items.Should().HaveCount(_warehouses.Count);
            page.Items.Select(w => w.Id).Should().Contain(_warehouses.Select(w => w.Id));
        }

        [Fact]
        public async Task GetWarehouses_ShouldReturnFirstPage_WhenPageSizeIsSmallerThanTotalCount()
        {
            var response = await Client.GetAsync("/api/v1/warehouses?pageNumber=1&pageSize=2", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var page = await response.Content.ReadFromJsonAsync<PagedResponse<WarehouseResponse>>(cancellationToken: CancellationToken.None);

            page.Should().NotBeNull();
            page!.Items.Should().HaveCount(2);
            page.PageNumber.Should().Be(1);
            page.PageSize.Should().Be(2);
            page.TotalCount.Should().Be(_warehouses.Count);
            page.TotalPages.Should().Be((int)Math.Ceiling(_warehouses.Count / 2.0));
            page.HasPreviousPage.Should().BeFalse();
            page.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetWarehouses_ShouldReturnRemainingItems_OnLastPage()
        {
            var response = await Client.GetAsync("/api/v1/warehouses?pageNumber=3&pageSize=2", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var page = await response.Content.ReadFromJsonAsync<PagedResponse<WarehouseResponse>>(cancellationToken: CancellationToken.None);

            page.Should().NotBeNull();
            page!.Items.Should().HaveCount(1);
            page.HasPreviousPage.Should().BeTrue();
            page.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetWarehouses_ShouldIncludeLinksForEachWarehouse()
        {
            var response = await Client.GetAsync("/api/v1/warehouses?pageNumber=1&pageSize=10", CancellationToken.None);

            var page = await response.Content.ReadFromJsonAsync<PagedResponse<WarehouseResponse>>(cancellationToken: CancellationToken.None);

            page.Should().NotBeNull();
            page!.Items.Should().OnlyContain(item =>
                item.Links.Select(link => link.Rel).Contains("self") &&
                item.Links.Select(link => link.Rel).Contains("delete-warehouse") &&
                item.Links.Select(link => link.Rel).Contains("create-warehouse"));
        }

        [Fact]
        public async Task GetWarehouses_WithInvalidToken_ReturnsUnauthorized()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/warehouses?pageNumber=1&pageSize=10");
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
