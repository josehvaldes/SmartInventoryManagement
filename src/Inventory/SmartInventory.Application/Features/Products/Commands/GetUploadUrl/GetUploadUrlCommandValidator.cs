using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Products.Commands.GetUploadUrl
{
    public class GetUploadUrlCommandValidator: AbstractValidator<GetUploadUrlCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".png" };

        public GetUploadUrlCommandValidator()
        {
            RuleFor(x => x.ProductSku)
                .NotEmpty().WithMessage("Product SKU is required.")
                .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("File name must not exceed 255 characters.");

            RuleFor(x => x.FileName)
                .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                .WithMessage($"Only the following file types are allowed: {string.Join(", ", AllowedExtensions)}");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("Content type is required.")
                .MaximumLength(100).WithMessage("Content type must not exceed 100 characters.");
        }
    }
}
