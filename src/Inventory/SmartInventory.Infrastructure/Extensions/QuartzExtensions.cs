using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartInventory.Infrastructure.BackgroundJobs;


namespace SmartInventory.Infrastructure.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration config) 
        {
            services.AddQuartz(q =>
            {

                // ---- LowStockCheckJob ----
                q.AddJob<LowStockCheckJob>(opts => opts
                    .WithIdentity(LowStockCheckJob.Key)
                    .WithDescription("Checks for low/below-reorder-point stock across all warehouses.")
                    .StoreDurably());

                q.AddTrigger(opts => opts
                    .ForJob(LowStockCheckJob.Key)
                    .WithIdentity("low-stock-check-trigger", "inventory")
                    .WithDescription("Fires every hour")
                    .WithCronSchedule("0 0/2 * ? * *"));

                // ---- ExpiredRefreshTokenCleanupJob ----
                q.AddJob<ExpiredRefreshTokenCleanupJob>(opts => opts
                    .WithIdentity(ExpiredRefreshTokenCleanupJob.Key)
                    .WithDescription("Purges refresh tokens outside the 30-day reuse-detection retention window.")
                    .StoreDurably());

                q.AddTrigger(opts => opts
                    .ForJob(ExpiredRefreshTokenCleanupJob.Key)
                    .WithIdentity("expired-refresh-token-cleanup-trigger", "auth")
                    .WithDescription("Fires once a day at 03:00 UTC")
                    .WithCronSchedule("0 0 3 * * ?"));
            });

            // This registers the hosted service that drives the Quartz scheduler.
            services.AddQuartzHostedService(opts =>
            {
                // Wait for running jobs to finish before the app shuts down.
                opts.WaitForJobsToComplete = true;
            });
            return services;
        }
    }
}
