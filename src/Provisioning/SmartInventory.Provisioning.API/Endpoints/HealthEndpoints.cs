namespace SmartInventory.Provisioning.API.Endpoints
{
    public static class HealthEndpoints
    {

        public static void MapHealthEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/", () =>"Server is running");
            app.MapHealthChecks("/healthz");
            app.MapGet("/health", () => "Healthy").RequireAuthorization();

            app.MapOpenApi();// add openapi/v1.json route
        }
    }
}
