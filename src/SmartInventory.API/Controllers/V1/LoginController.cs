using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Features.Auth.Commands;
using SmartInventory.Contracts.Requests.Login;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class LoginController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
    {

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            logger.LogInformation("Login attempt for user: {Username}", request.Username);
            try 
            {
                var response = await mediator.Send(new LoginCommand(request.Username, request.Password));
                return Ok(response);
            }
            catch(UnauthorizedAccessException ex)  
            {
                logger.LogWarning("Unauthorized login attempt for user: {Username}", request.Username);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
