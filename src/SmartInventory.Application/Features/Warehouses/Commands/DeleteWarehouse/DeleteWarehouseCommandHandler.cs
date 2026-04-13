using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Warehouses.Commands.DeleteWarehouse
{
    public class DeleteWarehouseCommandHandler(IApplicationDbContext db,
        ICacheService cache) : ICommandHandler<DeleteWarehouseCommand, Guid>
    {
        public async Task<Guid> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {

            var entity = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == request.guid, cancellationToken);

            if (entity == null) 
            { 
                throw EntityNotFoundException.For<Warehouse>(request.guid);
            }

            db.Warehouses.Remove(entity);
            var key = CacheKeys<WarehouseDto>.ById(request.guid);
            await cache.RemoveAsync(key);

            return request.guid;
        }
    }
}
