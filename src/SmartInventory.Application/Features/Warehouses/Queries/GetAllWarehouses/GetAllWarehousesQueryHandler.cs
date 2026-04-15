using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Models;
using SmartInventory.Application.Features.Warehouses.DTO;

namespace SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public class GetAllWarehousesQueryHandler(IApplicationDbContext db, ICacheService cache)
        : IQueryHandler<GetAllWarehousesQuery, PagedResult<WarehouseDto>>
    {
        public async Task<PagedResult<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await db.Warehouses.CountAsync(cancellationToken);

            var warehouses = await db.Warehouses
                .AsNoTracking()
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var warehouseDtos = warehouses.Adapt<List<WarehouseDto>>();
            var tasks = warehouseDtos.Select(x => cache.SetAsync(CacheKeys<WarehouseDto>.ById(x.Id), x, TimeSpan.FromMinutes(5)));
            await Task.WhenAll(tasks);

            return new PagedResult<WarehouseDto>(warehouseDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
