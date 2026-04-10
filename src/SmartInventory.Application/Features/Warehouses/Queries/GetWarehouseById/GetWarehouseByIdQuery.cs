using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Features.Warehouses.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Application.Features.Warehouses.Queries.GetWarehouseById
{
    public record GetWarehouseByIdQuery(Guid Id) : IQuery<WarehouseDto>
    {
    }
}
