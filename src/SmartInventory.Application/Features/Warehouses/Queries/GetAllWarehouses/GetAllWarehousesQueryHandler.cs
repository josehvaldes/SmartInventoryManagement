using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Domain.Entities;


namespace SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public class GetAllWarehousesQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetAllWarehousesQuery, List<WarehouseDto>>
    {
        public async Task<List<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
        {
            var warehouses = await db.Warehouses.AsNoTracking().ToListAsync(cancellationToken);
            var warehouseDtos = warehouses.Adapt<List<WarehouseDto>>();

            var tasks = warehouseDtos.Select(x => cache.SetAsync(CacheKeys<WarehouseDto>.ById(x.Id), x, TimeSpan.FromMinutes(5)));
            await Task.WhenAll(tasks);

            return warehouseDtos;
        }
    }
}
