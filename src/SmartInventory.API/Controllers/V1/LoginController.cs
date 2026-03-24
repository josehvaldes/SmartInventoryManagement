using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.Application.Features.Auth.Commands;
using SmartInventory.Contracts.Requests.Login;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth/[controller]")]
    [ApiVersion("1.0")]
    public class LoginController(IMediator mediator, ILogger<LoginController> logger) : ControllerBase
    {

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthEndpoints")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            logger.LogInformation("Login attempt for user: {Username}", request.Username);
            var response = await mediator.Send(new LoginCommand(request.Username, request.Password));
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            // Implement token refresh logic here if using JWT with refresh tokens.
            // TODO : Add RefreshTokenCommand and handle it in the AuthService to generate a new token based on the refresh token.
            //      : Add RefreshToken entity to store the tokens in database
            //       : Add logic to validate the refresh token and generate a new access token
            // For now, just return a placeholder response
            return Ok(new { Message = "Token refreshed successfully." });
        }
    }
}
