using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            var product = Product.Create(
                command.SKU,
                command.Name,
                command.Description,
                command.Category,
                command.UnitOfMeasure,
                command.MinimumStockLevel,
                command.ReorderPoint,
                command.ReorderQuantity,
                command.UnitCost,
                command.CreatedBy);

            db.Products.Add(product);

            return product.Id;
        }
    }
}
