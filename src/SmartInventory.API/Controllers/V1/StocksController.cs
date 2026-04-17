using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.Application.Features.Stocks.Commands;
using SmartInventory.Contracts.Requests.Stocks;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class StocksController(
        IMediator mediator,
        ILogger<StocksController> logger,
        IValidator<RemoveStockRequest> removeStockRequestValidator
        ) : ControllerBase
    {

        [HttpPost("removestock")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> RemoveStock(RemoveStockRequest request) 
        {
            (await removeStockRequestValidator.ValidateAsync(request)).ThrowIfInvalid();

            var command = new RemoveStockCommand(request.ProductId, request.WarehouseId, request.Quantity);
            logger.LogInformation("Removing stock: {@Command}", command);
            
            var response = await mediator.Send(command);

            return NoContent();
        }
    }
}
