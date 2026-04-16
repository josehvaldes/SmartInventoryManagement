using FluentAssertions;
using SmartInventory.API.Validators;
using SmartInventory.Contracts.Requests.Products;
using Xunit;

namespace SmartInventory.UnitTests.ApiValidators
{
    public class CreateProductRequestValidatorTests
    {
        private readonly CreateProductRequestValidator _validator;

        public CreateProductRequestValidatorTests()
        {
            _validator = new CreateProductRequestValidator();
        }

        private static CreateProductRequest ValidRequest() => new()
        {
            SKU = "SKU-001",
            Name = "Test Product",
            Description = "A valid description",
            Category = "Electronics",
            UnitOfMeasure = "Piece",
            UnitCost = 9.99m
        };

        // --- SKU ---

        [Fact]
        public void Should_Pass_When_Request_Is_Valid()
        {
            var result = _validator.Validate(ValidRequest());
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Fail_When_SKU_Is_Empty()
        {
            var request = ValidRequest();
            request.SKU = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.SKU) && e.ErrorMessage == "SKU is required.");
        }

        // --- Name ---

        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var request = ValidRequest();
            request.Name = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Name) && e.ErrorMessage == "Name is required.");
        }

        // --- Description ---

        [Fact]
        public void Should_Fail_When_Description_Is_Empty()
        {
            var request = ValidRequest();
            request.Description = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Description) && e.ErrorMessage == "Description is required.");
        }

        // --- Category ---

        [Fact]
        public void Should_Fail_When_Category_Is_Empty()
        {
            var request = ValidRequest();
            request.Category = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Category) && e.ErrorMessage == "Category is required.");
        }

        [Theory]
        [InlineData("Electronics")]
        [InlineData("Consumables")]
        [InlineData("Equipment")]
        [InlineData("Tools")]
        [InlineData("Safety")]
        [InlineData("RawMaterials")]
        [InlineData("FinishedGoods")]
        [InlineData("Packaging")]
        [InlineData("Other")]
        [InlineData("electronics")] // case-insensitive
        public void Should_Pass_When_Category_Is_Valid_Enum_Name(string category)
        {
            var request = ValidRequest();
            request.Category = category;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Category));
        }

        [Fact]
        public void Should_Fail_When_Category_Is_Not_A_Valid_Enum_Name()
        {
            var request = ValidRequest();
            request.Category = "InvalidCategory";

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.Category) && e.ErrorMessage.StartsWith("Category must be one of:"));
        }

        // --- UnitOfMeasure ---

        [Fact]
        public void Should_Fail_When_UnitOfMeasure_Is_Empty()
        {
            var request = ValidRequest();
            request.UnitOfMeasure = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(request.UnitOfMeasure) && e.ErrorMessage == "Unit of Measure is required.");
        }

        [Theory]
        [InlineData("Piece")]
        [InlineData("Box")]
        [InlineData("Pallet")]
        [InlineData("Kilogram")]
        [InlineData("Gram")]
        [InlineData("Liter")]
        [InlineData("Meter")]
        [InlineData("piece")] // case-insensitive
        public void Should_Pass_When_UnitOfMeasure_Is_Valid_Enum_Name(string unitOfMeasure)
        {
            var request = ValidRequest();
            request.UnitOfMeasure = unitOfMeasure;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.UnitOfMeasure));
        }

        [Fact]
        public void Should_Fail_When_UnitOfMeasure_Is_Not_A_Valid_Enum_Name()
        {
            var request = ValidRequest();
            request.UnitOfMeasure = "InvalidUnit";

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.UnitOfMeasure) && e.ErrorMessage.StartsWith("UnitOfMeasure must be one of:"));
        }

        // --- UnitCost ---

        [Fact]
        public void Should_Fail_When_UnitCost_Is_Zero()
        {
            var request = ValidRequest();
            request.UnitCost = 0;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.UnitCost) && e.ErrorMessage == "Unit Cost must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_UnitCost_Is_Negative()
        {
            var request = ValidRequest();
            request.UnitCost = -1m;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.UnitCost) && e.ErrorMessage == "Unit Cost must be greater than 0.");
        }

        [Fact]
        public void Should_Pass_When_UnitCost_Is_Greater_Than_Zero()
        {
            var request = ValidRequest();
            request.UnitCost = 0.01m;

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.UnitCost));
        }
    }
}
