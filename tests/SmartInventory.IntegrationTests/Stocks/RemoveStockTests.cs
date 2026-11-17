using FluentAssertions;
using Microsoft.Data.SqlClient;
using SmartInventory.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using SmartInventory.IntegrationTests.Products;
using SmartInventory.IntegrationTests.Warehouses;
using SmartInventory.Contracts.Requests.Stocks;
using SmartInventory.Domain.Entities;

namespace SmartInventory.IntegrationTests.Stocks
{
    [Collection("Integration")]
    public class RemoveStockTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
    {
        private Product _product = null!;
        private Warehouse _warehouse = null!;
        private Stock _stock = null!;

        protected override async Task SeedDataAsync(SqlConnection connection)
        {
            var products = ProductData.LoadProductsFromFile();
            var warehouses = WarehouseData.LoadWarehousesFromFile();
            var stocks = StocksData.LoadStocksFromFile();

            _stock = stocks.First();
            _product = products.First(p => p.Id == _stock.ProductId);
            _warehouse = warehouses.First(w => w.Id == _stock.WarehouseId);
            _stock.TenantId = SystemTenants.LegacyTenantId;
            _product.TenantId = SystemTenants.LegacyTenantId;
            _warehouse.TenantId = SystemTenants.LegacyTenantId;


            await using var context = CreateSmartInventoryDbContext();
            await context.Products.AddAsync(_product);
            await context.Warehouses.AddAsync(_warehouse);
            await context.Stocks.AddAsync(_stock);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task StockReduction_ShouldReturnSuccess()
        {
            var initialStockOnHand = _stock.QuantityOnHand;
            var initialAvailableStock = _stock.QuantityAvailable;
            var stockReductionRequest = new RemoveStockRequest(_product.Id, _warehouse.Id, 5);
            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            await using var context = CreateSmartInventoryDbContext();
            context.Stocks.Should().ContainSingle(s => s.ProductId == _product.Id && s.WarehouseId == _warehouse.Id);
            var updatedStock = context.Stocks.Single(s => s.ProductId == _product.Id && s.WarehouseId == _warehouse.Id);
            updatedStock.QuantityOnHand.Should().Be(initialStockOnHand - 5);
            updatedStock.QuantityAvailable.Should().Be(initialAvailableStock - 5);

        }

        [Fact]
        public async Task StockReduction_WhenQuantityEqualsAvailableStock_ReducesStockToZero()
        {
            var initialAvailableStock = _stock.QuantityAvailable;
            var stockReductionRequest = new RemoveStockRequest(_product.Id, _warehouse.Id, initialAvailableStock);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var context = CreateSmartInventoryDbContext();
            var updatedStock = context.Stocks.Single(s => s.ProductId == _product.Id && s.WarehouseId == _warehouse.Id);
            updatedStock.QuantityAvailable.Should().Be(0);
        }

        [Fact]
        public async Task StockReduction_WhenQuantityExceedsAvailableStock_ReturnsInternalServerError()
        {
            var initialStockOnHand = _stock.QuantityOnHand;
            var initialAvailableStock = _stock.QuantityAvailable;
            var stockReductionRequest = new RemoveStockRequest(_product.Id, _warehouse.Id, initialAvailableStock + 1);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            // Stock should remain unchanged since the operation failed
            await using var context = CreateSmartInventoryDbContext();
            var updatedStock = context.Stocks.Single(s => s.ProductId == _product.Id && s.WarehouseId == _warehouse.Id);
            updatedStock.QuantityOnHand.Should().Be(initialStockOnHand);
            updatedStock.QuantityAvailable.Should().Be(initialAvailableStock);
        }

        [Fact]
        public async Task StockReduction_WhenStockDoesNotExistForProductAndWarehouse_ReturnsNotFound()
        {
            var stockReductionRequest = new RemoveStockRequest(Guid.NewGuid(), Guid.NewGuid(), 5);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task StockReduction_WhenProductIdIsEmpty_ReturnsUnprocessableEntity()
        {
            var stockReductionRequest = new RemoveStockRequest(Guid.Empty, _warehouse.Id, 5);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task StockReduction_WhenWarehouseIdIsEmpty_ReturnsUnprocessableEntity()
        {
            var stockReductionRequest = new RemoveStockRequest(_product.Id, Guid.Empty, 5);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task StockReduction_WhenQuantityIsNotPositive_ReturnsUnprocessableEntity(decimal invalidQuantity)
        {
            var stockReductionRequest = new RemoveStockRequest(_product.Id, _warehouse.Id, invalidQuantity);

            var response = await Client.PostAsJsonAsync("/api/v1/stocks/reduction", stockReductionRequest, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task StockReduction_WithInvalidToken_ReturnsUnauthorized()
        {
            var stockReductionRequest = new RemoveStockRequest(_product.Id, _warehouse.Id, 5);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/stocks/reduction")
            {
                Content = JsonContent.Create(stockReductionRequest)
            };
            request.Headers.Authorization = new("Bearer", TestAuthHandler.InvalidToken);

            var response = await Client.SendAsync(request, CancellationToken.None);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
