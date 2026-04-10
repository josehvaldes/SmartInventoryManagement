using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse
{
    public class CreateWarehouseCommandHandler(IApplicationDbContext db)
        : ICommandHandler<CreateWarehouseCommand, Guid>
    {
        public async Task<Guid> Handle(CreateWarehouseCommand command, CancellationToken cancellationToken)
        {
            Address? address = null;
            if (!string.IsNullOrWhiteSpace(command.Street) &&
                !string.IsNullOrWhiteSpace(command.City) &&
                !string.IsNullOrWhiteSpace(command.Country))
            {
                address = new Address
                {
                    Street = command.Street,
                    City = command.City,
                    State = command.State ?? string.Empty,
                    PostalCode = command.PostalCode ?? string.Empty,
                    Country = command.Country
                };
            }

            var warehouse = Warehouse.Create(
                command.Code,
                command.Name,
                address,
                command.WarehouseType,
                command.Capacity,
                command.ManagerName,
                command.ManagerEmail,
                command.ManagerPhone,
                command.CreatedBy);

            db.Warehouses.Add(warehouse);

            return warehouse.Id;
        }
    }
}
