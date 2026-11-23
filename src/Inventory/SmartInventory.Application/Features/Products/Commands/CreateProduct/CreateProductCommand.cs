using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Enums;

namespace SmartInventory.Application.Features.Products.Commands.CreateProduct
{
    public record CreateProductCommand(
        string SKU,
        string Name,
        string Description,
        ProductCategory Category,
        UnitOfMeasure UnitOfMeasure,
        decimal MinimumStockLevel,
        decimal ReorderPoint,
        decimal ReorderQuantity,
        decimal? UnitCost
    ) : ICommand<Guid>;
}
