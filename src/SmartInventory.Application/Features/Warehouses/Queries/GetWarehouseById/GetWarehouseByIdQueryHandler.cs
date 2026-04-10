using Mapster;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Warehouses.Queries.GetWarehouseById
{
    public class GetWarehouseByIdQueryHandler(IApplicationDbContext db, ICacheService cache) : IQueryHandler<GetWarehouseByIdQuery, WarehouseDto>
    {
        public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        {
            var key = $"Warehouse:{request.Id}";
            var cachedWarehouse = await cache.GetAsync<WarehouseDto>(key);

            if (cachedWarehouse!=null)
                return cachedWarehouse;
            
            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (warehouse == null)
                throw EntityNotFoundException.For<Warehouse>(request.Id);

            var dto = warehouse.Adapt<WarehouseDto>();
            await cache.SetAsync(key, dto, TimeSpan.FromMinutes(5));

            return dto;
        }
    }
}
