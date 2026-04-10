using Asp.Versioning;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using SmartInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;
using SmartInventory.Application.Features.Warehouses.Queries.GetWarehouseById;
using SmartInventory.Contracts.Requests.Warehouses;
using SmartInventory.Contracts.Responses.Warehouses;
using SmartInventory.Domain.Entities;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]

    public class WarehousesController(IMediator mediator,
            ILogger<WarehousesController> logger,
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
            logger.LogInformation("Warehouse created successfully with ID: {Id}", id);
            return Created($"warehouses/{id}", new { Id = id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            logger.LogInformation("Received request to get all warehouses");
            var response = await mediator.Send(new GetAllWarehousesQuery());
            return Ok(response.Adapt<List<WarehouseResponse>>());
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            logger.LogInformation("Received request to get warehouse with ID: {Id}", id);
            var response = await mediator.Send(new GetWarehouseByIdQuery(id));
            return Ok(response.Adapt<WarehouseResponse>());
        }
    }
}
