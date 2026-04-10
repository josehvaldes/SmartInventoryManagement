using Asp.Versioning;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.Application.Features.Products.Commands.CreateProduct;
using SmartInventory.Application.Features.Products.Commands.DeleteProduct;
using SmartInventory.Application.Features.Products.Commands.GetUploadUrl;
using SmartInventory.Application.Features.Products.Commands.UploadProduct;
using SmartInventory.Application.Features.Products.Queries.GetProductById;
using SmartInventory.Application.Features.Products.Queries.GetProducts;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Contracts.Responses.Products;
using ValidationException = SmartInventory.Application.Common.Exceptions.ValidationException;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductsController(IMediator mediator,
        ILogger<ProductsController> logger,
        IValidator<CreateProductRequest> productValidator,
        IValidator<IFormFile> fileValidator,
        IValidator<UploadProductRequest> uploadRequestValidator,
        IValidator<GetUploadUrlRequest> getUploadUrlRequestValidator
        ) : ControllerBase
    {

        [HttpPost]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {            
            var validationResult = await productValidator.ValidateAsync(request);
            validationResult.ThrowIfInvalid();

            var command = request.Adapt<CreateProductCommand>();
            var id = await mediator.Send(command);

            return Created($"products/{id}", new { Id = id });
        }


        [HttpPost("upload")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductData([FromForm] UploadProductRequest request, IFormFile file)
        {
            if (file is null)
                throw new ValidationException(new Dictionary<string, string[]> { [nameof(file)] = ["A file is required."] });

            (await fileValidator.ValidateAsync(file)).ThrowIfInvalid();

            (await uploadRequestValidator.ValidateAsync(request)).ThrowIfInvalid();

            var updatedBy = User.Identity?.Name ?? "Unknown";
            var command = new UploadProductCommand( request.ProductSKU, request.ProductName, file, updatedBy);
            logger.LogInformation("Received file {FileName} for product data upload.", file.FileName);
            var result = await mediator.Send(command);

            return Ok(new { ImageUrl = result });
        }

        [HttpPost("upload-url")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> GetUploadUrl(GetUploadUrlRequest request)
        {
            (await getUploadUrlRequestValidator.ValidateAsync(request)).ThrowIfInvalid();
            var command = new GetUploadUrlCommand(request.ProductSku, request.FileName, request.ContentType);
            var url = await mediator.Send(command);
            return Ok(new { UploadUrl = url });
        }


        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            logger.LogInformation("Received request to delete product with ID: {ProductId}", id);
            var command = new DeleteProductCommand(id);
            await mediator.Send(command);
            logger.LogInformation("Product with ID: {ProductId} has been deleted.", id);
            return Ok(new { Id = id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<ProductResponse>> Get()
        {
            logger.LogInformation("Received request to get all products.");
            var response = await mediator.Send(new GetProductsQuery());
            return response.Adapt<List<ProductResponse>>();
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            logger.LogInformation("Received request to get product with ID: {ProductId}", id);
            var command = new GetProductByIdQuery(id);
            var response = await mediator.Send(command);
            return Ok(response.Adapt<ProductResponse>());
        }
    }
}
