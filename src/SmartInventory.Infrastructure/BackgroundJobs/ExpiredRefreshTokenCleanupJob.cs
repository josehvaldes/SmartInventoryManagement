using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Infrastructure.Settings;

namespace SmartInventory.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Purges refresh tokens that are both expired/revoked AND outside the reuse-detection
    /// window (30 days past expiry). Keeping tokens until this window elapses allows the
    /// system to detect and respond to replay attacks even after the token has expired.
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class ExpiredRefreshTokenCleanupJob(
        IAuthDbContext db,
        ILogger<ExpiredRefreshTokenCleanupJob> logger,
        IOptions<JwtSettings> opts) : IJob
    {
        public static readonly JobKey Key = new("expired-refresh-token-cleanup", "auth");

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("ExpiredRefreshTokenCleanupJob started at {Time}", DateTimeOffset.UtcNow);
            try
            {
                var cutoff = DateTime.UtcNow - TimeSpan.FromDays(opts.Value.RefreshTokenRetentionDays);

                var deleted = await db.RefreshTokens
                    .Where(rt => rt.ExpiresAt <= cutoff)
                    .ExecuteDeleteAsync(context.CancellationToken);

                logger.LogInformation(
                    "ExpiredRefreshTokenCleanupJob deleted {Count} token(s) older than {Cutoff:O}.",
                    deleted, cutoff);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ExpiredRefreshTokenCleanupJob failed.");
            }
        }
    }
}
