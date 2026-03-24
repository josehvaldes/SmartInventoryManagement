using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Stocks.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Queries
{
    public class GetStockByProductIdQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetStockByProductIdQuery, StockDto>
    {
        public async Task<StockDto> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
        {
            var key = "GetStockByProductIdQueryHandler_" + request.productId;

            var stockDto = await cache.GetAsync<StockDto>(key);
            if (stockDto != null)
            {
                return stockDto;
            }

            var entity = await db.Stocks.FirstOrDefaultAsync(s => s.ProductId == request.productId, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Stock not found for product ID: {request.productId}");
            }
            stockDto = entity.Adapt<StockDto>();
            await cache.SetAsync(key, stockDto, TimeSpan.FromMinutes(5));
            return stockDto;
        }
    }
}
