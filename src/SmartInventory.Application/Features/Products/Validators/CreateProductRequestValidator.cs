using FluentValidation;
using SmartInventory.Contracts.Requests.Products;

namespace SmartInventory.API.Validators
{
    public class ProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public ProductRequestValidator() 
        {
            RuleFor(x => x.SKU).NotEmpty().WithMessage("SKU is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
            RuleFor(x => x.UnitOfMeasure).NotEmpty().WithMessage("Unit of Measure is required.");
            RuleFor(x => x.UnitCost).GreaterThan(0).WithMessage("Unit Cost must be greater than 0.");
        }
    }
}
