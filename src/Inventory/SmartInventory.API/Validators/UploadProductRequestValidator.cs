using FluentValidation;
using SmartInventory.Contracts.Requests.Products;

namespace SmartInventory.API.Validators
{
    public class UploadProductRequestValidator : AbstractValidator<UploadProductRequest>
    {
        public UploadProductRequestValidator() 
        {
            RuleFor(x => x.ProductSKU)
                .NotEmpty().WithMessage("Product SKU is required.")
                .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters.");
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
        }
    }
}
