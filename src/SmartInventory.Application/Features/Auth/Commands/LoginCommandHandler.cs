using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Responses.Auth;
using SmartInventory.Domain.Identity;

namespace SmartInventory.Application.Features.Auth.Commands
{
    public class LoginCommandHandler(IAuthDbContext db, 
        IJwtTokenService jwt, 
        IPasswordHasher<User> hasher,
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

            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var token = jwt.GenerateToken(user, roles);

            return new LoginResponse(token, user.Username);
        }
    }
}
