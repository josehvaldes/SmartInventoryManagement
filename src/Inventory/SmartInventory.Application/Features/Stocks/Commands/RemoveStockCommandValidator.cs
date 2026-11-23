using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Commands
{
    public class RemoveStockCommandValidator: AbstractValidator<RemoveStockCommand>
    {
        public RemoveStockCommandValidator() 
        {
            RuleFor(x => x.productId).NotEmpty().WithMessage("ProductId is required.");
            RuleFor(x => x.warehouseId).NotEmpty().WithMessage("WarehouseId is required.");
            RuleFor(x => x.quantityToRemove).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }
}
