using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Application.Features.Products.Queries.GetAllProducts;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.Products
{
    public class GetProductsQueryHandlerTests
    {
        private IApplicationDbContext _db;
        private readonly GetAllProductsQueryHandler _handler;
        private readonly ICacheService _cache;
        private readonly List<Product> _products;

        public GetProductsQueryHandlerTests()
        {
            _products = ProductLoader.LoadProductsFromFile();
            var mockProductsSet = MockDbSetHelper.CreateMockDbSet(_products);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Products.Returns(mockProductsSet);

            _cache = Substitute.For<ICacheService>();
            _cache.GetAsync<ProductDto>(Arg.Any<string>()).Returns((ProductDto?)null); // Simulate cache miss

            _handler = new GetAllProductsQueryHandler(_db, _cache);
        }

        [Fact]
        public async Task GetProductsQueryHandler_Returns_List_of_Products()
        {
            // Arrange
            var query = new GetAllProductsQuery(PageNumber: 1, PageSize: 20);
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            // Assert
            result.Should().NotBeNull();
            result.Items.Count.Should().Be(_products.Count);
        }

        [Fact]
        public async Task Handle_Returns_Empty_List_When_No_Products_Exist()
        {
            // Arrange
            var emptySet = MockDbSetHelper.CreateMockDbSet(new List<Product>());
            _db.Products.Returns(emptySet);
            var query = new GetAllProductsQuery(PageNumber: 1, PageSize: 20);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }
    }
}
