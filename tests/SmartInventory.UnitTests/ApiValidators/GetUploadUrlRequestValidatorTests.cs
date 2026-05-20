using FluentAssertions;
using SmartInventory.API.Validators;
using SmartInventory.Contracts.Requests.Products;
using Xunit;

namespace SmartInventory.UnitTests.ApiValidators
{
    public class GetUploadUrlRequestValidatorTests
    {
        private readonly GetUploadUrlRequestValidator _validator;

        public GetUploadUrlRequestValidatorTests()
        {
            _validator = new GetUploadUrlRequestValidator();
        }

        private static GetUploadUrlRequest ValidRequest() => new()
        {
            ProductSku = "SKU-001",
            FileName = "image.png",
            ContentType = "image/png"
        };

        [Fact]
        public void Should_Pass_When_Request_Is_Valid()
        {
            var result = _validator.Validate(ValidRequest());
            result.IsValid.Should().BeTrue();
        }

        // --- ProductSku ---

        [Fact]
        public void Should_Fail_When_ProductSku_Is_Empty()
        {
            var request = ValidRequest();
            request.ProductSku = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductSku) && e.ErrorMessage == "Product SKU is required.");
        }

        [Fact]
        public void Should_Fail_When_ProductSku_Exceeds_50_Characters()
        {
            var request = ValidRequest();
            request.ProductSku = new string('A', 51);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ProductSku) && e.ErrorMessage == "Product SKU must not exceed 50 characters.");
        }

        [Fact]
        public void Should_Pass_When_ProductSku_Is_Exactly_50_Characters()
        {
            var request = ValidRequest();
            request.ProductSku = new string('A', 50);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ProductSku));
        }

        // --- FileName ---

        [Fact]
        public void Should_Fail_When_FileName_Is_Empty()
        {
            var request = ValidRequest();
            request.FileName = "";

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.FileName) && e.ErrorMessage == "File name is required.");
        }

        [Fact]
        public void Should_Fail_When_FileName_Exceeds_255_Characters()
        {
            var request = ValidRequest();
            request.FileName = string.Concat(Enumerable.Repeat("a", 256));

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.FileName) && e.ErrorMessage == "File name must not exceed 255 characters.");
        }

        [Fact]
        public void Should_Pass_When_FileName_Is_Exactly_255_Characters()
        {
            var request = ValidRequest();
            request.FileName = string.Concat(Enumerable.Repeat("a", 255));

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.FileName));
        }

        // --- ContentType ---

        [Fact]
        public void Should_Fail_When_ContentType_Is_Empty()
        {
            var request = ValidRequest();
            request.ContentType = string.Empty;

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ContentType) && e.ErrorMessage == "Content type is required.");
        }

        [Fact]
        public void Should_Fail_When_ContentType_Exceeds_100_Characters()
        {
            var request = ValidRequest();
            request.ContentType = new string('a', 101);

            var result = _validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(request.ContentType) && e.ErrorMessage == "Content type must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Pass_When_ContentType_Is_Exactly_100_Characters()
        {
            var request = ValidRequest();
            request.ContentType = new string('a', 100);

            var result = _validator.Validate(request);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ContentType));
        }
    }
}
