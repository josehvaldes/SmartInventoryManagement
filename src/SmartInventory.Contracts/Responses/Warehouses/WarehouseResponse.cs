using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventory.Contracts.Responses.Warehouses
{
    public class WarehouseResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WarehouseType { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public decimal? Capacity { get; set; }

        public IReadOnlyList<Link> Links { get; set; } = [];

    }
}
