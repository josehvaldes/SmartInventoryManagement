using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Common.Exceptions;
using SmartInventory.Application.Common.Interfaces;
using SmartInventory.Contracts.Responses.Auth;
using SmartInventory.Domain.Identity;
using SmartInventory.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
            try
            {

                var user = await db.Users
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == cmd.Username, ct)
                    ?? throw new EntityNotFoundException("Invalid credentials");
                
                if (hasher.VerifyHashedPassword(user, user.PasswordHash, cmd.Password) == PasswordVerificationResult.Failed)
                    throw new UnauthorizedAccessException("Invalid credentials"); // same message — don't leak which field failed

                var roles = user.UserRoles.Select(ur => ur.Role.Name);
                var token = jwt.GenerateToken(user, roles);

                return new LoginResponse(token, user.Username);
            }
            catch (Exception ex) when (ex is EntityNotFoundException || ex is UnauthorizedAccessException)
            {
                logger.LogWarning(ex, "Login attempt failed for username: {Username}", cmd.Username);
                throw new UnauthorizedAccessException("Invalid credentials");
            }
            catch( Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while processing the login request for username: {Username}", cmd.Username);
                throw new Exception("An error occurred while processing the login request", ex);
            }
        }
    }
}
