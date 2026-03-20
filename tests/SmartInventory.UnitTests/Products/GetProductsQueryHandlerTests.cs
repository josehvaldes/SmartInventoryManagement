using FluentAssertions;
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
    public class GetProductsQueryHandlerTests
    {
        private IApplicationDbContext _db;
        private readonly GetProductsQueryHandler _handler;
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

            _handler = new GetProductsQueryHandler(_db);
        }

        [Fact]
        public async Task GetProductsQueryHandler_Returns_List_of_Products()
        {
            // Arrange
            var query = new GetProductsQuery();
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(_products.Count);
        }
    }
}
