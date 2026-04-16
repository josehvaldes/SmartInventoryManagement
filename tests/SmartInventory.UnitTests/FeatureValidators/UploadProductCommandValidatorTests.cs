using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using SmartInventory.Application.Features.Products.Commands.UploadProduct;
using Xunit;

namespace SmartInventory.UnitTests.FeatureValidators
{
    public class UploadProductCommandValidatorTests
    {
        private readonly UploadProductCommandValidator _validator;

        public UploadProductCommandValidatorTests()
        {
            _validator = new UploadProductCommandValidator();
        }

        private static IFormFile CreateMockFile(
            string fileName = "product.jpg",
            string contentType = "image/jpeg",
            long length = 1024)
        {
            var file = Substitute.For<IFormFile>();
            file.FileName.Returns(fileName);
            file.ContentType.Returns(contentType);
            file.Length.Returns(length);
            return file;
        }

        private static UploadProductCommand ValidCommand() => new(
            productSKU: "SKU-001",
            productName: "Test Product",
            file: CreateMockFile(),
            updatedBy: "test-user"
        );

        // --- Valid command ---

        [Fact]
        public void Should_Pass_When_Command_Is_Valid()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_When_File_Has_Png_Extension()
        {
            var command = ValidCommand() with { file = CreateMockFile(fileName: "product.png", contentType: "image/png") };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- file (null) ---

        [Fact]
        public void Should_Fail_When_File_Is_Null()
        {
            var command = ValidCommand() with { file = null! };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(command.file) &&
                e.ErrorMessage == "File is required.");
        }

        // --- file.Length ---

        [Fact]
        public void Should_Fail_When_File_Is_Empty()
        {
            var command = ValidCommand() with { file = CreateMockFile(length: 0) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "file.Length" &&
                e.ErrorMessage == "File must not be empty.");
        }

        [Fact]
        public void Should_Fail_When_File_Exceeds_10MB()
        {
            const long overLimit = 10 * 1024 * 1024 + 1;
            var command = ValidCommand() with { file = CreateMockFile(length: overLimit) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "file.Length" &&
                e.ErrorMessage == "File size must not exceed 10MB.");
        }

        [Fact]
        public void Should_Pass_When_File_Is_Exactly_10MB()
        {
            const long exactly10MB = 10 * 1024 * 1024;
            var command = ValidCommand() with { file = CreateMockFile(length: exactly10MB) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_When_File_Is_1_Byte()
        {
            var command = ValidCommand() with { file = CreateMockFile(length: 1) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- file.ContentType ---

        [Fact]
        public void Should_Fail_When_File_ContentType_Is_Empty()
        {
            var command = ValidCommand() with { file = CreateMockFile(contentType: string.Empty) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == "file.ContentType" &&
                e.ErrorMessage == "File content type is required.");
        }

        // --- file.FileName extension ---

        [Theory]
        [InlineData("image.bmp")]
        [InlineData("image.gif")]
        [InlineData("image.webp")]
        [InlineData("document.pdf")]
        [InlineData("archive.zip")]
        public void Should_Fail_When_File_Has_Disallowed_Extension(string fileName)
        {
            var command = ValidCommand() with { file = CreateMockFile(fileName: fileName) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == "file.FileName" &&
                e.ErrorMessage == "Only the following file types are allowed: .jpg, .png");
        }

        [Theory]
        [InlineData("image.jpg")]
        [InlineData("image.JPG")]
        [InlineData("image.png")]
        [InlineData("image.PNG")]
        public void Should_Pass_When_File_Extension_Is_Case_Insensitive(string fileName)
        {
            var command = ValidCommand() with { file = CreateMockFile(fileName: fileName) };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        // --- productSKU ---

        [Fact]
        public void Should_Fail_When_ProductSKU_Is_Empty()
        {
            var command = ValidCommand() with { productSKU = string.Empty };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.productSKU) &&
                e.ErrorMessage == "Product SKU is required.");
        }

        // --- productName ---

        [Fact]
        public void Should_Fail_When_ProductName_Is_Empty()
        {
            var command = ValidCommand() with { productName = string.Empty };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(command.productName) &&
                e.ErrorMessage == "Product name is required.");
        }
    }
}
