using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.UploadProduct
{
    public class UploadProductCommandValidator : AbstractValidator<UploadProductCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public UploadProductCommandValidator() 
        {
            RuleFor(x => x.file)
                .NotNull().WithMessage("File is required.");

            RuleFor(x => x.file.Length)
                .GreaterThan(0).WithMessage("File must not be empty.")
                .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File size must not exceed 10MB.");

            RuleFor(x => x.file.ContentType)
                .NotEmpty().WithMessage("File content type is required.");

            RuleFor(x => x.file.FileName)
                .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                .WithMessage($"Only the following file types are allowed: {string.Join(", ", AllowedExtensions)}");

            RuleFor(x => x.productSKU)
                .NotEmpty().WithMessage("Product SKU is required.");

            RuleFor(x => x.productName)
                .NotEmpty().WithMessage("Product name is required.");
        }
    }
}
