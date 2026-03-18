using Mapster;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler(IApplicationDbContext db) : IQueryHandler<GetProductByIdQuery, ProductDto>
    {
        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await db.Products.FindAsync(request.Id, cancellationToken);

            if (product == null)
                throw EntityNotFoundException.For<Product>(request.Id);
            
            return product.Adapt<ProductDto>();
        }
    }
}
