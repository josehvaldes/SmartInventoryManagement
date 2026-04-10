using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse
{
    public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateWarehouseCommandValidator(IApplicationDbContext db)
        {
            _context = db;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Warehouse code is required.")
                .MaximumLength(20).WithMessage("Warehouse code must not exceed 20 characters.")
                .MustAsync(BeUniqueCode).WithMessage("Warehouse code must be unique.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required.")
                .MaximumLength(200).WithMessage("Warehouse name must not exceed 200 characters.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be a positive value.")
                .When(x => x.Capacity.HasValue);

            RuleFor(x => x.ManagerName)
                .MaximumLength(100).WithMessage("Manager name must not exceed 100 characters.");

            RuleFor(x => x.ManagerEmail)
                .EmailAddress().WithMessage("A valid manager email address is required.")
                .When(x => !string.IsNullOrWhiteSpace(x.ManagerEmail));

            RuleFor(x => x.ManagerPhone)
                .MaximumLength(20).WithMessage("Manager phone must not exceed 20 characters.");
        }

        private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
        {
            return !await _context.Warehouses.AnyAsync(w => w.Code == code, cancellationToken);
        }
    }
}
