using FluentValidation;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Domain.Enums;

namespace SmartInventory.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator() 
        {
            RuleFor(x => x.SKU).NotEmpty().WithMessage("SKU is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required.")
                .IsEnumName(typeof(ProductCategory), caseSensitive: false)
                .WithMessage($"Category must be one of: {string.Join(", ", Enum.GetNames<ProductCategory>())}");
            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of Measure is required.")
                .IsEnumName(typeof(UnitOfMeasure), caseSensitive: false)
                .WithMessage($"UnitOfMeasure must be one of: {string.Join(", ", Enum.GetNames<UnitOfMeasure>())}");
            RuleFor(x => x.UnitCost).GreaterThan(0).WithMessage("Unit Cost must be greater than 0.");
        }
    }
}
