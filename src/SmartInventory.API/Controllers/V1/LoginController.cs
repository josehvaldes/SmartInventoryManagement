using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.Application.Features.Auth.Commands;
using SmartInventory.Contracts.Requests.Login;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [EnableCors("StrictCors")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    public class LoginController(IMediator mediator,
        IValidator<LoginRequest> loginValidator,
        ILogger<LoginController> logger) : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";

        /// <summary>
        /// OAuth-style token endpoint: validates credentials, returns an access token in the
        /// response body and a refresh token in an HttpOnly Secure cookie (not accessible to JS).
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthEndpoints")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var validationResult = await loginValidator.ValidateAsync(request);
            validationResult.ThrowIfInvalid();

            logger.LogInformation("Login attempt for user: {Username}", request.Username);
            var response = await mediator.Send(new LoginCommand(request.Username, request.Password));

            SetRefreshTokenCookie(response.RawRefreshToken!, response.RefreshTokenExpiry);

            return Ok(response);
        }

        /// <summary>
        /// Issues a new access token using the refresh token stored in the HttpOnly cookie.
        /// Implements refresh token rotation: the old refresh token is revoked and a new one is issued.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthEndpoints")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new { Message = "No refresh token provided." });

            var response = await mediator.Send(new RefreshTokenCommand(refreshToken));

            SetRefreshTokenCookie(response.RawRefreshToken!, response.RefreshTokenExpiry);

            return Ok(response);
        }

        /// <summary>
        /// Revokes the refresh token stored in the cookie (logout).
        /// Clears the HttpOnly cookie on the client.
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await mediator.Send(new RevokeTokenCommand(refreshToken));

            Response.Cookies.Delete(RefreshTokenCookieName);
            logger.LogInformation("User logged out, refresh token revoked.");
            return NoContent();
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,          // not accessible to JavaScript — prevents XSS token theft
                Secure = true,            // only sent over HTTPS
                SameSite = SameSiteMode.Strict, // CSRF protection
                Expires = expires
            });
        }
    }
}
