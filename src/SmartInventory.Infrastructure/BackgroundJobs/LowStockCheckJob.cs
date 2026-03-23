using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;

namespace SmartInventory.Infrastructure.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public sealed class LowStockCheckJob(
        //IApplicationDbContext db,
        ILogger<LowStockCheckJob> logger) : IJob
    {
        public static readonly JobKey Key = new("low-stock-check", "inventory");
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("LowStockCheckJob started at {Time}", DateTimeOffset.UtcNow);
            try
            {
                logger.LogInformation("LowStockCheckJob completed at {Time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                // Log but don't rethrow — Quartz will mark the trigger as failed if we throw,
                // which can block future executions depending on the misfire policy.
                logger.LogError(ex, "LowStockCheckJob failed");
            }
        }
    }
}
