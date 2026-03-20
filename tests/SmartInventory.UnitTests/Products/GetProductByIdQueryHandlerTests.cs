using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Application.Features.Products.Queries.GetProductById;
using SmartInventory.Application.Features.Products.Queries.GetProducts;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
