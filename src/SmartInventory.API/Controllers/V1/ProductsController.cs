using Asp.Versioning;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.Application.Features.Products.Commands.CreateProduct;
using SmartInventory.Application.Features.Products.Queries.GetProducts;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Contracts.Responses.Products;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductsController(IMediator mediator,
        ILogger<ProductsController> logger,
        IValidator<CreateProductRequest> productValidator) : ControllerBase
    {

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<ProductResponse>> Get()
        {
            logger.LogInformation("Received request to get all products.");
            var response = await mediator.Send(new GetProductsQuery());
            logger.LogInformation("Returning {Count} products.", response.Count);
            return response.Adapt<List<ProductResponse>>();
        }

        [HttpPost]
        [EnableRateLimiting("WriteOperations")]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {            
            var validationResult = await productValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Product creation failed validation: {Errors}", validationResult.Errors);
                return BadRequest(validationResult.Errors);
            }

            var command = request.Adapt<CreateProductCommand>();
            var id = await mediator.Send(command);

            return Ok(id);
        }
    }
}
