using SmartInventory.Domain.Enums;

namespace SmartInventory.Application.Features.Warehouses.DTO
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public WarehouseType WarehouseType { get; set; }
        public decimal? Capacity { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
