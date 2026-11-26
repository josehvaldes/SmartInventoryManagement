using System.Text.Json.Serialization;

namespace SmartInventory.Provisioning.API.Configuration
{
    public static class HttpConfiguration
    {
        public static void ConfigureHttpPipeline(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            });
        }
    }
}
