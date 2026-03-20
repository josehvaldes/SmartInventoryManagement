using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using SmartInventory.Infrastructure.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.Text;

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
                    .StoreDurably()); // keep the job definition even if no trigger is attached

                q.AddTrigger(opts => opts
                    .ForJob(LowStockCheckJob.Key)
                    .WithIdentity("low-stock-check-trigger", "inventory")
                    .WithDescription("Fires every hour")
                    .WithCronSchedule("0 0/2 * ? * *")); // every 3 minutes, on the hour (UTC). Move to config if needed.
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
