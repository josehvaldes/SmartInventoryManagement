using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Features.Warehouses.Commands.DeleteWarehouse
{
    public record DeleteWarehouseCommand(Guid guid) : ICommand<Guid>;
    
}
