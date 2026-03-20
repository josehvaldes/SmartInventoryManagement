using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Stocks.Commands;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.Stocks
{
    public class GetStockByProductIdQueryHandlerTests
    {
        private readonly IApplicationDbContext _db;
        private readonly GetStockByProductIdQueryHandler _handler;
        private readonly List<Stock> _stocks;

        public GetStockByProductIdQueryHandlerTests()
        {
            _stocks = StockLoader.LoadStocksFromFile();
            var mockStocksSet = MockDbSetHelper.CreateMockDbSet(_stocks);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Stocks.Returns(mockStocksSet);

            _handler = new GetStockByProductIdQueryHandler(_db, null!);
        }

        [Fact]
        public async Task Handle_Returns_StockDto_When_ProductId_Exists()
        {
            // Arrange
            var stock = _stocks.First();
            var query = new GetStockByProductIdQuery(stock.ProductId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(stock.ProductId);
            result.QuantityOnHand.Should().Be(stock.QuantityOnHand);
            result.QuantityReserved.Should().Be(stock.QuantityReserved);
            result.QuantityAvailable.Should().Be(stock.QuantityAvailable);
        }

        [Fact]
        public async Task Handle_Throws_KeyNotFoundException_When_ProductId_Does_Not_Exist()
        {
            // Arrange
            var query = new GetStockByProductIdQuery(Guid.NewGuid());

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Stock not found for product ID*");
        }
    }
}
