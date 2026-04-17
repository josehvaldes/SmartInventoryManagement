using FluentValidation;
using SmartInventory.Contracts.Requests.Stocks;

namespace SmartInventory.API.Validators
{
    public class RemoveStockRequestValidator : AbstractValidator<RemoveStockRequest>
    {
        public RemoveStockRequestValidator() 
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
            RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("WarehouseId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }
}
