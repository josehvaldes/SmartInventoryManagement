using FluentAssertions;
using SmartInventory.API.Validators;
using SmartInventory.Contracts.Requests.Products;
using Xunit;

namespace SmartInventory.UnitTests.ApiValidators
{
    public class UploadProductRequestValidatorTests
    {
        private readonly UploadProductRequestValidator _validator;

        public UploadProductRequestValidatorTests()
        {
            _validator = new UploadProductRequestValidator();
        }

        private static UploadProductRequest ValidRequest() => new()
        {
            ProductSKU = "SKU-001",
            ProductName = "Test Product"
        };

        [Fact]
        public void Should_Pass_When_Request_Is_Valid()
        {
            var result = _validator.Validate(ValidRequest());
            result.IsValid.Should().BeTrue();
        }

        // --- ProductSKU ---

        [Fact]
        public void Should_Fail_When_ProductSKU_Is_Empty()
        {
            var request = ValidRequest();
            request.ProductSKU = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductSKU) && e.ErrorMessage == "Product SKU is required.");
        }

        [Fact]
        public void Should_Fail_When_ProductSKU_Exceeds_50_Characters()
        {
            var request = ValidRequest();
            request.ProductSKU = new string('A', 51);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductSKU) && e.ErrorMessage == "Product SKU must not exceed 50 characters.");
        }

        [Fact]
        public void Should_Pass_When_ProductSKU_Is_Exactly_50_Characters()
        {
            var request = ValidRequest();
            request.ProductSKU = new string('A', 50);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ProductSKU));
        }

        // --- ProductName ---

        [Fact]
        public void Should_Fail_When_ProductName_Is_Empty()
        {
            var request = ValidRequest();
            request.ProductName = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductName) && e.ErrorMessage == "Product name is required.");
        }

        [Fact]
        public void Should_Fail_When_ProductName_Exceeds_100_Characters()
        {
            var request = ValidRequest();
            request.ProductName = new string('A', 101);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductName) && e.ErrorMessage == "Product name must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Pass_When_ProductName_Is_Exactly_100_Characters()
        {
            var request = ValidRequest();
            request.ProductName = new string('A', 100);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ProductName));
        }
    }
}
