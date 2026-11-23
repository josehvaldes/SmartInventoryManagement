using SmartInventory.Contracts.Responses;

namespace SmartInventory.API.Services
{
    public interface ILinkService
    {
        IReadOnlyList<Link> GetWarehouseLinks(Guid id);
        IReadOnlyList<Link> GetProductLinks(Guid id);
    }
}
