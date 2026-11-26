namespace SmartInventory.Provisioning.API.Endpoints
{
    public static class TenantsEndpoints
    {

        public static void MapTenantsEndpoints(this IEndpointRouteBuilder app)
        {
            // Map tenant-related endpoints here
            app.MapGet("/tenants", () => "List of tenants").RequireAuthorization();
            app.MapGet("/tenantrequests", () => "List of tenant requests").RequireAuthorization();
        }
    }
}
