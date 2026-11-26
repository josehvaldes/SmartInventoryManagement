
namespace SmartInventory.Provisioning.API.Endpoints
{
    public static class ApiEndpoints
    {
        public static void MapApiEndpoints(this WebApplication app) 
        {
            app.MapHealthEndpoints();
            

            var v1 = app.NewVersionedApi().MapGroup("/api/v{version:apiVersion}")
                        .HasApiVersion(1.0);
            
            v1.MapPublicEndpoints();
            v1.MapTenantsEndpoints();
        }
    }
}
