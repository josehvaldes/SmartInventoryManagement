using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class LoginCommandHandler(IAuthDbContext db,
        IJwtTokenService jwt,
        IPasswordHasher<User> hasher,
        ITenantContext tenantContext,
        ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand cmd, CancellationToken ct)
        {
            var user = await db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == cmd.Username, ct);

            if (user == null || hasher.VerifyHashedPassword(user, user.PasswordHash, cmd.Password) == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Failed login attempt for username: {Username}", cmd.Username);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var tenantMembership = db.TenantMemberships.AsNoTracking().Where(tm => tm.UserId == user.Id && tenantContext.ResolvedTenantId == tm.TenantId).Include(tm => tm.Tenant).FirstOrDefault();
            if (tenantMembership == null || !tenantMembership.IsActive || tenantMembership.Tenant.Status != TenantStatus.Active)
            {
                throw new TenantException(TenantErrorCodes.ContextRequired, $"User {user.Id} has no active membership in tenant {tenantContext.ResolvedTenantId}.", StatusCodes.Status403Forbidden);
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var accessToken = jwt.GenerateAccessToken(user, tenantMembership, roles);
            var (refreshTokenValue, refreshTokenExpiry) = jwt.GenerateRefreshToken();

            db.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiry
            });

            return new LoginResponse(accessToken, user.Username, jwt.AccessTokenExpirySeconds)
            {
                RawRefreshToken = refreshTokenValue,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }
    }
}
