using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Stocks.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Commands
{
    public class GetStockByProductIdQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetStockByProductIdQuery, StockDto>
    {
        public async Task<StockDto> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await db.Stocks.FirstOrDefaultAsync(s => s.ProductId == request.productId, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Stock not found for product ID: {request.productId}");
            }

            return entity.Adapt<StockDto>();
        }
    }
}
