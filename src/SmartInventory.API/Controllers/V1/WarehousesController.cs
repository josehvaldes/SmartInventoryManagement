using Asp.Versioning;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.API.Services;
using SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using SmartInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;
using SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;
using SmartInventory.Application.Features.Warehouses.Queries.GetWarehouseById;
using SmartInventory.Contracts.Requests;
using SmartInventory.Contracts.Requests.Warehouses;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Warehouses;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]

    public class WarehousesController(IMediator mediator,
            ILogger<WarehousesController> logger,
            ILinkService linkService,
            IValidator<CreateWarehouseRequest> warehouseValidator
        ) : ControllerBase
    {
        [HttpPost]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> CreateWarehouse(CreateWarehouseRequest request)
        {
            logger.LogInformation("Received request to create warehouse with code: {Code}", request.Code);
            var validationResult = await warehouseValidator.ValidateAsync(request);
            validationResult.ThrowIfInvalid();

            var command = request.Adapt<CreateWarehouseCommand>();
            var id = await mediator.Send(command);
            var links = linkService.GetWarehouseLinks(id);
            logger.LogInformation("Warehouse created successfully with ID: {Id}", id);
            return CreatedAtAction(nameof(GetWarehouse), new { id }, new { Id = id, Links = links });
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> DeleteWarehouse(Guid id) 
        {
            var command = new DeleteWarehouseCommand(id);
            await mediator.Send(command);
            logger.LogInformation("Warehouse with ID: {Id} has been deleted.", id);
            return NoContent();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] GetPagingRequest request)
        {
            logger.LogInformation("Received request to get all warehouses");
            var pagedResult = await mediator.Send(new GetAllWarehousesQuery(request.PageNumber, request.PageSize));

            var items = pagedResult.Items.Adapt<List<WarehouseResponse>>();
            foreach (var item in items)
                item.Links = linkService.GetWarehouseLinks(item.Id);

            return Ok(new PagedResponse<WarehouseResponse>(
                items,
                pagedResult.TotalCount,
                pagedResult.PageNumber,
                pagedResult.PageSize,
                pagedResult.TotalPages,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetWarehouse(Guid id)
        {
            logger.LogInformation("Received request to get warehouse with ID: {Id}", id);
            var response = await mediator.Send(new GetWarehouseByIdQuery(id));
            var warehouseResponse = response.Adapt<WarehouseResponse>();
            warehouseResponse.Links = linkService.GetWarehouseLinks(warehouseResponse.Id);
            return Ok(warehouseResponse);
        }
    }
}
