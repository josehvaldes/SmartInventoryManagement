using SmartInventory.Application.Common.Cache;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler(IApplicationDbContext db, 
        ICacheService cache) : ICommandHandler<DeleteProductCommand, Guid>
    {
        public async Task<Guid> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var entity = db.Products.FirstOrDefault(x => x.Id == request.guid);
            if (entity == null)
                throw EntityNotFoundException.For<Product>(request.guid);

            db.Products.Remove(entity);
            var key = CacheKeys<ProductDto>.ById(request.guid);
            await cache.RemoveAsync(key);

            return request.guid;
        }
    }
}
