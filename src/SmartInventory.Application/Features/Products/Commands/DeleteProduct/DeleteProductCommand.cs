using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Features.Products.Commands.DeleteProduct
{
    public record class DeleteProductCommand(Guid guid) : ICommand<Guid>;
    
}
