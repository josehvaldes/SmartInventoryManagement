using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Requests.Stocks
{
    public record RemoveStockRequest(Guid ProductId, Guid WarehouseId, decimal Quantity);
}
