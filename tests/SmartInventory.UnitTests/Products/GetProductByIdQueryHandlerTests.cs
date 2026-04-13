using FluentAssertions;
using Mapster;
using Newtonsoft.Json;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Application.Features.Products.Queries.GetProductById;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.Products
{

    public class GetProductByIdQueryHandlerTests
    {
        
        private IApplicationDbContext _db;
        private readonly GetProductByIdQueryHandler _handler;
        private readonly ICacheService _cache;
        private readonly List<Product> _products;

        public GetProductByIdQueryHandlerTests() 
        {
            _products = ProductLoader.LoadProductsFromFile();
            var mockProductsSet = MockDbSetHelper.CreateMockDbSet(_products);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Products.Returns(mockProductsSet);
            
            _cache = Substitute.For<ICacheService>();
            _cache.GetAsync<ProductDto>(Arg.Any<string>()).Returns((ProductDto?)null); // Simulate cache miss

            _handler = new GetProductByIdQueryHandler(_db, _cache);
        }

        [Fact]
        public async Task Handle_Return_Product_by_ID_success() 
        {
            var product = _products.FirstOrDefault() ?? throw new InvalidOperationException("No products available for testing.");
            // Arrange
            var query = new GetProductByIdQuery(product.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(product.Id);
        }

        [Fact]
        public async Task Handle_Returns_Product_From_Cache_When_Cache_Hit()
        {
            // Arrange
            var product = _products.First();
            var cachedDto = product.Adapt<ProductDto>();
            _cache.GetAsync<ProductDto>(CacheKeys<ProductDto>.ById(product.Id)).Returns(cachedDto);
            var query = new GetProductByIdQuery(product.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeSameAs(cachedDto);
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Throws_EntityNotFoundException_When_Product_Not_Found()
        {
            // Arrange
            var query = new GetProductByIdQuery(Guid.NewGuid());

            // Act
            var act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Product*");
        }

        [Fact]
        public async Task Handle_Stores_Product_In_Cache_After_DB_Hit()
        {
            // Arrange
            var product = _products.First();
            var query = new GetProductByIdQuery(product.Id);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _cache.Received(1).SetAsync(
                CacheKeys<ProductDto>.ById(product.Id),
                Arg.Any<ProductDto>(),
                TimeSpan.FromMinutes(5));
        }
    }
}
