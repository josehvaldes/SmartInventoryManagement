using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.Commands.CreateProduct;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.UnitTests.Common;
using Xunit;

namespace SmartInventory.UnitTests.FeatureValidators
{
    public class CreateProductCommandValidatorTests
    {
        private readonly IApplicationDbContext _context;
        private readonly CreateProductCommandValidator _validator;

        public CreateProductCommandValidatorTests()
        {
            _context = Substitute.For<IApplicationDbContext>();
            _validator = new CreateProductCommandValidator(_context);
        }

        private static CreateProductCommand ValidCommand() => new(
            SKU: "SKU-001",
            Name: "Test Product",
            Description: "A valid description",
            Category: ProductCategory.Electronics,
            UnitOfMeasure: UnitOfMeasure.Piece,
            MinimumStockLevel: 10,
            ReorderPoint: 5,
            ReorderQuantity: 20,
            UnitCost: 9.99m
        );

        private void SetupProductsDbSet(List<Product> products)
        {
            var mockSet = MockDbSetHelper.CreateMockDbSet(products);
            _context.Products.Returns(mockSet);
        }

        // --- Valid command ---

        [Fact]
        public async Task Should_Pass_When_Command_Is_Valid()
        {
            SetupProductsDbSet([]);

            var result = await _validator.ValidateAsync(ValidCommand(), TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Name ---

        [Fact]
        public async Task Should_Fail_When_Name_Is_Empty()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Name = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Name) &&
                e.ErrorMessage == "Product name is required.");
        }

        [Fact]
        public async Task Should_Fail_When_Name_Exceeds_100_Characters()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Name = new string('A', 101) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Name) &&
                e.ErrorMessage == "Product name must not exceed 100 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_Name_Is_Exactly_100_Characters()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Name = new string('A', 100) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Description ---

        [Fact]
        public async Task Should_Fail_When_Description_Exceeds_500_Characters()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Description = new string('D', 501) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.Description) &&
                e.ErrorMessage == "Product description must not exceed 500 characters.");
        }

        [Fact]
        public async Task Should_Pass_When_Description_Is_Exactly_500_Characters()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Description = new string('D', 500) };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_Description_Is_Empty()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { Description = string.Empty };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- UnitCost ---

        [Fact]
        public async Task Should_Fail_When_UnitCost_Is_Negative()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { UnitCost = -0.01m };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.UnitCost) &&
                e.ErrorMessage == "Unit Cost must be a non-negative value.");
        }

        [Fact]
        public async Task Should_Pass_When_UnitCost_Is_Zero()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { UnitCost = 0m };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_UnitCost_Is_Null()
        {
            SetupProductsDbSet([]);
            var command = ValidCommand() with { UnitCost = null };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- SKU uniqueness ---

        [Fact]
        public async Task Should_Fail_When_SKU_Already_Exists()
        {
            var existingSku = "SKU-001";
            var products = new List<Product> { CreateProductWithSku(existingSku) };
            SetupProductsDbSet(products);

            var command = ValidCommand() with { SKU = existingSku };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.SKU) &&
                e.ErrorMessage == "SKU must be unique.");
        }

        [Fact]
        public async Task Should_Pass_When_SKU_Is_Unique()
        {
            var products = new List<Product> { CreateProductWithSku("SKU-999") };
            SetupProductsDbSet(products);

            var command = ValidCommand() with { SKU = "SKU-001" };

            var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Pass_When_No_Products_Exist()
        {
            SetupProductsDbSet([]);

            var result = await _validator.ValidateAsync(ValidCommand(), TestContext.Current.CancellationToken);

            result.IsValid.Should().BeTrue();
        }

        // --- Helper ---

        private static Product CreateProductWithSku(string sku)
        {
            return Product.Create(
                sku: sku,
                name: "Existing Product",
                description: "Existing",
                category: ProductCategory.Other,
                unitOfMeasure: UnitOfMeasure.Piece,
                minimumStockLevel: 1,
                reorderPoint: 1,
                reorderQuantity: 1,
                unitCost: null,
                createdBy: "seed"
            );
        }
    }
}
