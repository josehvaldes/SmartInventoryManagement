using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.API.Models;
using SmartInventory.Application.Features.Products.Queries.GetProducts;

namespace SmartInventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IMediator mediator): ControllerBase
    {

        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            var response = await mediator.Send(new GetProductsQuery());

            return response.Adapt<List<Product>>();
        }
    }
}
