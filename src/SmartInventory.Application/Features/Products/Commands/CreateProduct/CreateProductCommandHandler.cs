using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id          = Guid.NewGuid(),
                SKU         = command.SKU,
                Name        = command.Name,
                Description = command.Description,
                Category    = command.Category,
                UnitOfMeasure     = command.UnitOfMeasure,
                MinimumStockLevel = command.MinimumStockLevel,
                ReorderPoint      = command.ReorderPoint,
                ReorderQuantity   = command.ReorderQuantity,
                UnitCost    = command.UnitCost,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow,
                CreatedBy   = command.CreatedBy,
                UpdatedBy   = command.CreatedBy
            };

            db.Products.Add(product);
            await db.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
