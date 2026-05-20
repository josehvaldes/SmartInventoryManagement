using Asp.Versioning;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartInventory.API.Extensions;
using SmartInventory.API.Services;
using SmartInventory.Application.Features.Products.Commands.CreateProduct;
using SmartInventory.Application.Features.Products.Commands.DeleteProduct;
using SmartInventory.Application.Features.Products.Commands.GetUploadUrl;
using SmartInventory.Application.Features.Products.Commands.UploadProduct;
using SmartInventory.Application.Features.Products.Queries.GetProductById;
using SmartInventory.Application.Features.Products.Queries.GetAllProducts;
using SmartInventory.Contracts.Requests;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Contracts.Responses;
using SmartInventory.Contracts.Responses.Products;
using ValidationException = SmartInventory.Application.Common.Exceptions.ValidationException;

namespace SmartInventory.API.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductsController(IMediator mediator,
        ILogger<ProductsController> logger,
        ILinkService linkService,
        IValidator<CreateProductRequest> productValidator,
        IValidator<IFormFile> fileValidator,
        IValidator<UploadProductRequest> uploadRequestValidator,
        IValidator<GetUploadUrlRequest> getUploadUrlRequestValidator
        ) : ControllerBase
    {
        [ValidateAntiForgeryToken]
        [HttpPost]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {            
            var validationResult = await productValidator.ValidateAsync(request);
            validationResult.ThrowIfInvalid();

            var command = request.Adapt<CreateProductCommand>();

            var id = await mediator.Send(command);
            var links = linkService.GetProductLinks(id);
            
            return CreatedAtAction(nameof(GetProduct), new { id }, new { Id = id, Links = links });
        }


        [HttpPost("upload")]
        [EnableRateLimiting("WriteOperations")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]                          // 10 MB hard cap
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
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
            return NoContent();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] GetPagingRequest request)
        {
            logger.LogInformation("Received request to get all products.");
            var pagedResult = await mediator.Send(new GetAllProductsQuery(request.PageNumber, request.PageSize));

            var items = pagedResult.Items.Adapt<List<ProductResponse>>();
            foreach (var item in items)
                item.Links = linkService.GetProductLinks(item.Id);

            return Ok(new PagedResponse<ProductResponse>(
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
        public async Task<IActionResult> GetProduct(Guid id)
        {
            logger.LogInformation("Received request to get product with ID: {ProductId}", id);
            var command = new GetProductByIdQuery(id);
            var response = await mediator.Send(command);
            var result = response.Adapt<ProductResponse>();
            result.Links = linkService.GetProductLinks(id);

            return Ok(result);
        }
    }
}
