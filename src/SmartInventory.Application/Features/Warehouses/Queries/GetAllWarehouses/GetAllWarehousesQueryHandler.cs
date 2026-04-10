using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public class GetAllWarehousesQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetAllWarehousesQuery, List<WarehouseDto>>
    {
        public async Task<List<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
        {
            var key = $"{nameof(GetAllWarehousesQueryHandler)}:GetAllWarehouses";

            var cachedData = await cache.GetAsync<List<WarehouseDto>>(key);
            if (cachedData != null) { 
                return cachedData;
            }

            var warehouses = await db.Warehouses.AsNoTracking().ToListAsync(cancellationToken);
            var warehouseDtos = warehouses.Adapt<List<WarehouseDto>>();
            await cache.SetAsync(key, warehouseDtos);
            return warehouseDtos;
        }
    }
}
