using MediatR;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Commands
{
    public class RemoveStockCommandHandler(IApplicationDbContext db) : ICommandHandler<RemoveStockCommand>
    {
        public async Task<Unit> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
        {
            var stock = db.Stocks.FirstOrDefault(s => s.ProductId == request.productId && s.WarehouseId == request.warehouseId);
            if (stock == null)
            {
                throw EntityNotFoundException.For<Stock>($"Stock not found for ProductId: {request.productId} and WarehouseId: {request.warehouseId}");
            }
            stock.RemoveStock(request.quantityToRemove, Guid.NewGuid());
            return default;
        }
    }
}
