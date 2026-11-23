using MediatR;
using SmartInventory.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Commands
{
    public record RemoveStockCommand (
        Guid productId,
        Guid warehouseId,
        decimal quantityToRemove
        ) : ICommand;
    
}
