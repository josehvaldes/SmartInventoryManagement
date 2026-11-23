using FluentValidation;
using SmartInventory.Contracts.Requests.Products;

namespace SmartInventory.API.Validators
{
    public class GetUploadUrlRequestValidator : AbstractValidator<GetUploadUrlRequest>
    {

        public GetUploadUrlRequestValidator() 
        {
            RuleFor(x => x.ProductSku)
                .NotEmpty().WithMessage("Product SKU is required.")
                .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters.");
            RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("File name must not exceed 255 characters.");
            RuleFor(x => x.ContentType).NotEmpty().WithMessage("Content type is required.")
                .MaximumLength(100).WithMessage("Content type must not exceed 100 characters.");
        }
    }
}
