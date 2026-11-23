using SmartInventory.Contracts.Responses;

namespace SmartInventory.API.Services
{
    public class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ILinkService
    {
        public IReadOnlyList<Link> GetWarehouseLinks(Guid id)
        {
            var httpContext = httpContextAccessor.HttpContext!;

            return [
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "GetWarehouse", "Warehouses", new { id }) ?? string.Empty,
                    "self",
                    "GET"),
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "DeleteWarehouse", "Warehouses", new { id }) ?? string.Empty,
                    "delete-warehouse",
                    "DELETE"),
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "CreateWarehouse", "Warehouses", null) ?? string.Empty,
                    "create-warehouse",
                    "POST")
            ];
        }

        public IReadOnlyList<Link> GetProductLinks(Guid id)
        {
            var httpContext = httpContextAccessor.HttpContext!;

            return
            [
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "GetProduct", "Products", new { id }) ?? string.Empty,
                    "self",
                    "GET"),
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "DeleteProduct", "Products", new { id }) ?? string.Empty,
                    "delete-product",
                    "DELETE"),
                new Link(
                    linkGenerator.GetUriByAction(httpContext, "CreateProduct", "Products", null) ?? string.Empty,
                    "create-product",
                    "POST")
            ];
        }
    }
}

