using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Application.Common.Tenants;
using SmartInventory.Contracts.Responses.Auth;
using SmartInventory.Domain.Enums;
using SmartInventory.Domain.Identity;
using SmartInventory.Identity.Abstractions.Persistence;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public class RefreshTokenCommandHandler(
        IAuthDbContext db,
        IJwtTokenService jwt,
        ITenantContext tenantContext,
        ILogger<RefreshTokenCommandHandler> logger)
        : ICommandHandler<RefreshTokenCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(RefreshTokenCommand cmd, CancellationToken ct)
        {
            var existing = await db.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == cmd.RefreshToken, ct);

            if (existing is null || existing.ExpiresAt <= DateTime.UtcNow)
            {
                logger.LogWarning("Unknown or expired refresh token used.");
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            if (existing.IsRevoked)
            {
                // A revoked token was presented — this indicates a replay attack or token theft.
                // Revoke every token in the family (all tokens descended from the same root)
                // to force the legitimate user to re-authenticate.
                await RevokeTokenFamilyAsync(existing.UserId, ct);
                logger.LogWarning(
                    "Refresh token reuse detected for user {UserId}. Entire token family revoked.",
                    existing.UserId);
                throw new UnauthorizedAccessException("Token reuse detected. Please log in again.");
            }

            // Rotate: revoke the old token and issue a new one (refresh token rotation)
            existing.IsRevoked = true;

            var tenantMembership = db.TenantMemberships.AsNoTracking()
                .Where(tm => tm.UserId == existing.User.Id && tenantContext.ResolvedTenantId == tm.TenantId)
                .Include(tm => tm.Tenant).FirstOrDefault();

            if (tenantMembership == null || !tenantMembership.IsActive || tenantMembership.Tenant.Status != TenantStatus.Active)
            {
                throw new TenantException(TenantErrorCodes.ContextRequired, $"User {existing.User.Id} has no active membership in tenant {tenantContext.ResolvedTenantId}.", StatusCodes.Status403Forbidden);
            }


            var roles = existing.User.UserRoles.Select(ur => ur.Role.Name);
            var newAccessToken = jwt.GenerateAccessToken(existing.User, tenantMembership, roles);
            var (newRefreshTokenValue, newRefreshTokenExpiry) = jwt.GenerateRefreshToken();

            existing.ReplacedByToken = newRefreshTokenValue;

            db.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshTokenValue,
                UserId = existing.UserId,
                ExpiresAt = newRefreshTokenExpiry
            });

            return new LoginResponse(newAccessToken, existing.User.Username, jwt.AccessTokenExpirySeconds)
            {
                RawRefreshToken = newRefreshTokenValue,
                RefreshTokenExpiry = newRefreshTokenExpiry
            };
        }

        // Revokes all active tokens belonging to the user — used when a reuse attack is detected.
        // Targets only non-expired tokens to keep the UPDATE set small.
        private async Task RevokeTokenFamilyAsync(Guid userId, CancellationToken ct)
        {
            var activeTokens = await db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

            foreach (var t in activeTokens)
                t.IsRevoked = true;
        }
    }
}
