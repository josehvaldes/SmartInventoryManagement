namespace SmartInventory.API
{
    public static class KestrelConfiguration
    {
        /// <summary>
        /// Set a reasonable maximum request body size to prevent abuse and ensure efficient resource usage.
        /// </summary>
        private static readonly int MaxRequestBodySizeInBytes = 20 * 1024 * 1024; // 20 MB
        public static WebApplicationBuilder ConfigureKestrel(this WebApplicationBuilder builder)
        {
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = MaxRequestBodySizeInBytes;
            });

            return builder;
        }
    }
}
