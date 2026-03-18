using Mapster;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Contracts.Responses.Products;

namespace SmartInventory.API.Mappings
{
    public static class MappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ProductDto, ProductResponse>
                .NewConfig()
                .Map(dest => dest.Category,      src => src.Category.ToString())
                .Map(dest => dest.UnitOfMeasure, src => src.UnitOfMeasure.ToString());


        }
    }
}
