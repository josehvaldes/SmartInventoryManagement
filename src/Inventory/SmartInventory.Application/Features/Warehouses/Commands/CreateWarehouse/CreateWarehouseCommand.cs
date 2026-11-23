using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Enums;

namespace SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse
{
    public record CreateWarehouseCommand(
        string Code,
        string Name,
        WarehouseType WarehouseType,
        string? Street,
        string? City,
        string? State,
        string? PostalCode,
        string? Country,
        decimal? Capacity,
        string ManagerName,
        string ManagerEmail,
        string ManagerPhone
    ) : ICommand<Guid>;
}
