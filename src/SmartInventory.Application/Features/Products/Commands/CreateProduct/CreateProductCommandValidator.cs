using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Requests.Products;


namespace SmartInventory.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        IApplicationDbContext _context;
        public CreateProductCommandValidator(IApplicationDbContext db)
        {
            _context = db;
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Product description must not exceed 500 characters.");
            RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("Unit Cost must be a non-negative value.");
            RuleFor(x => x.SKU)
                .MustAsync(BeUniqueSKU).WithMessage("SKU must be unique."); // Ensure SKU is unique
        }

        private async Task<bool> BeUniqueSKU(string sku, CancellationToken cancellationToken)
        {
            return !await _context.Products.AnyAsync(p => p.SKU == sku, cancellationToken);
        }
    }
}
