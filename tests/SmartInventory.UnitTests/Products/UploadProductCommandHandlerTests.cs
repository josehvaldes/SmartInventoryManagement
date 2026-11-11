using FluentAssertions;
using NSubstitute;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.Commands.UploadProduct;
using SmartInventory.Domain.Entities;
using SmartInventory.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmartInventory.UnitTests.Products
{
    public class UploadProductCommandHandlerTests
    {
        private IApplicationDbContext _db;
        private readonly ICacheService _cache;
        private readonly IFileStorageService _fileStorageService;
        private readonly List<Product> _products;


        public UploadProductCommandHandlerTests()
        {
            _products = ProductLoader.LoadProductsFromFile();
            var mockProductsSet = MockDbSetHelper.CreateMockDbSet(_products);
            _db = Substitute.For<IApplicationDbContext>();
            _db.Products.Returns(mockProductsSet);
            _cache = Substitute.For<ICacheService>();

            _fileStorageService = Substitute.For<IFileStorageService>();
        }

        [Fact]
        public async Task Handle_UploadProductCommand_Should_Update_Product_ImageUrl()
        {
            // Arrange
            var product = _products.First();
            var command = new UploadProductCommand(product.SKU,
                product.Name,
                new MockFormFile("test.jpg", "image/jpeg"),
                "test_user");

            _fileStorageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns("https://example.com/test.jpg");
            var handler = new UploadProductCommandHandler(_fileStorageService, _db);
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            // Assert
            result.Should().Be("https://example.com/test.jpg");
            product.ImageUrl.Should().Be("https://example.com/test.jpg");
        }
    }
}
