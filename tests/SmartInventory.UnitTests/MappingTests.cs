using Mapster;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using Xunit;

namespace SmartInventory.UnitTests
{
    public class MappingTests
    {
        [Fact]
        public void ProductToProductDto_Mapping_IsValid()
        {
            // Arrange
            var product = Product.Create(
                sku: "TEST123",
                name: "Test Product",
                description: "This is a test product.",
                category: ProductCategory.Electronics,
                unitOfMeasure: UnitOfMeasure.Piece,
                minimumStockLevel: 10,
                reorderPoint: 20,
                reorderQuantity: 50,
                unitCost: 9.99m,
                createdBy: "testuser");
            // Act
            var productDto = product.Adapt<ProductDto>();
            // Assert
            Assert.Equal(product.Id, productDto.Id);
            Assert.Equal(product.SKU, productDto.SKU);
            Assert.Equal(product.Name, productDto.Name);
            Assert.Equal(product.Description, productDto.Description);
            Assert.Equal(product.Category, productDto.Category);
            Assert.Equal(product.UnitOfMeasure, productDto.UnitOfMeasure);
            Assert.Equal(product.MinimumStockLevel, productDto.MinimumStockLevel);
            Assert.Equal(product.ReorderPoint, productDto.ReorderPoint);
            Assert.Equal(product.ReorderQuantity, productDto.ReorderQuantity);
            Assert.Equal(product.UnitCost, productDto.UnitCost);
            Assert.Equal(product.IsActive, productDto.IsActive);
        }


        [Fact]
        public void CustomConfiguration_Mapping_IsValid()
        {
            TypeAdapterConfig<Product, ProductDto>.NewConfig()
                .Map(dest => dest.Name, src => src.Name.ToUpper());

            Product product = Product.Create(
                sku: "TEST123",
                name: "Test Product",
                description: "This is a test product.",
                category: ProductCategory.Electronics,
                unitOfMeasure: UnitOfMeasure.Piece,
                minimumStockLevel: 10,
                reorderPoint: 20,
                reorderQuantity: 50,
                unitCost: 9.99m,
                createdBy: "testuser");

            var productDto = product.Adapt<ProductDto>();
        }

        
    }
}   