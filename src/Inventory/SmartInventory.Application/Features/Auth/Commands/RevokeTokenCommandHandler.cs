using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Interfaces;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public class RevokeTokenCommandHandler(
        IAuthDbContext db,
        ILogger<RevokeTokenCommandHandler> logger)
        : IRequestHandler<RevokeTokenCommand, Unit>
    {
        public async Task<Unit> Handle(RevokeTokenCommand cmd, CancellationToken ct)
        {
            var token = await db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == cmd.RefreshToken, ct);

            if (token is null || token.IsRevoked)
            {
                logger.LogWarning("Revoke attempted on unknown or already-revoked token.");
                // Return silently — do not reveal token existence to the caller
                return Unit.Value;
            }

            token.IsRevoked = true;

            logger.LogInformation("Refresh token revoked for user {UserId}.", token.UserId);
            return Unit.Value;
        }
    }
}
