using FluentAssertions;
using SmartInventory.Application.Features.Products.Commands.GetUploadUrl;
using Xunit;

namespace SmartInventory.UnitTests.FeatureValidators
{
    public class GetUploadUrlCommandValidatorTests
    {
        private readonly GetUploadUrlCommandValidator _validator;

        public GetUploadUrlCommandValidatorTests()
        {
            _validator = new GetUploadUrlCommandValidator();
        }

        private static GetUploadUrlCommand ValidCommand() => new(
            ProductSku: "SKU-001",
            FileName: "product-image.jpg",
            ContentType: "image/jpeg"
        );

        // --- Valid command ---

        [Fact]
        public void Should_Pass_When_Command_Is_Valid()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_When_FileName_Has_Png_Extension()
        {
            var command = ValidCommand() with { FileName = "product-image.png" };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- ProductSku ---

        [Fact]
        public void Should_Fail_When_ProductSku_Is_Empty()
        {
            var command = ValidCommand() with { ProductSku = string.Empty };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ProductSku) &&
                e.ErrorMessage == "Product SKU is required.");
        }

        [Fact]
        public void Should_Fail_When_ProductSku_Exceeds_50_Characters()
        {
            var command = ValidCommand() with { ProductSku = new string('A', 51) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ProductSku) &&
                e.ErrorMessage == "Product SKU must not exceed 50 characters.");
        }

        [Fact]
        public void Should_Pass_When_ProductSku_Is_Exactly_50_Characters()
        {
            var command = ValidCommand() with { ProductSku = new string('A', 50) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- FileName ---

        [Fact]
        public void Should_Fail_When_FileName_Is_Empty()
        {
            var command = ValidCommand() with { FileName = string.Empty };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(command.FileName) &&
                e.ErrorMessage == "File name is required.");
        }

        [Fact]
        public void Should_Fail_When_FileName_Exceeds_255_Characters()
        {
            var command = ValidCommand() with { FileName = new string('A', 252) + ".jpg" };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.FileName) &&
                e.ErrorMessage == "File name must not exceed 255 characters.");
        }

        [Fact]
        public void Should_Pass_When_FileName_Is_Exactly_255_Characters()
        {
            var command = ValidCommand() with { FileName = new string('A', 251) + ".jpg" };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- FileName extension ---

        [Theory]
        [InlineData("image.bmp")]
        [InlineData("image.gif")]
        [InlineData("image.webp")]
        [InlineData("document.pdf")]
        [InlineData("archive.zip")]
        public void Should_Fail_When_FileName_Has_Disallowed_Extension(string fileName)
        {
            var command = ValidCommand() with { FileName = fileName };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.FileName) &&
                e.ErrorMessage == "Only the following file types are allowed: .jpg, .png");
        }

        [Theory]
        [InlineData("image.jpg")]
        [InlineData("image.JPG")]
        [InlineData("image.PNG")]
        [InlineData("image.png")]
        public void Should_Pass_When_FileName_Extension_Is_Case_Insensitive(string fileName)
        {
            var command = ValidCommand() with { FileName = fileName };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- ContentType ---

        [Fact]
        public void Should_Fail_When_ContentType_Is_Empty()
        {
            var command = ValidCommand() with { ContentType = string.Empty };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ContentType) &&
                e.ErrorMessage == "Content type is required.");
        }

        [Fact]
        public void Should_Fail_When_ContentType_Exceeds_100_Characters()
        {
            var command = ValidCommand() with { ContentType = new string('x', 101) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.ContentType) &&
                e.ErrorMessage == "Content type must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Pass_When_ContentType_Is_Exactly_100_Characters()
        {
            var command = ValidCommand() with { ContentType = new string('x', 100) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }
    }
}
