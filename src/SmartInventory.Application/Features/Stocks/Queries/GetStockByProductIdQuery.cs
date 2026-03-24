using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Stocks.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Stocks.Queries
{
    public record GetStockByProductIdQuery(Guid productId): IQuery<StockDto> { }
}
