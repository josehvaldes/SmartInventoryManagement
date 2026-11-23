using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Models;
using SmartInventory.Application.Features.Warehouses.DTO;

namespace SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public record GetAllWarehousesQuery(int PageNumber, int PageSize) : IQuery<PagedResult<WarehouseDto>>;
}
