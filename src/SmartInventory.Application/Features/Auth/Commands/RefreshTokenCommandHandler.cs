using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Responses.Auth;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public class RefreshTokenCommandHandler(
        IAuthDbContext db,
        IJwtTokenService jwt,
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

            var roles = existing.User.UserRoles.Select(ur => ur.Role.Name);
            var newAccessToken = jwt.GenerateAccessToken(existing.User, roles);
            var (newRefreshTokenValue, newRefreshTokenExpiry) = jwt.GenerateRefreshToken();

            existing.ReplacedByToken = newRefreshTokenValue;

            db.RefreshTokens.Add(new Domain.Identity.RefreshToken
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
