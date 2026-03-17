using Mapster;
using SmartInventory.Application.DTOs;

namespace SmartInventory.API.Mappings
{
    public static class MappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ProductDto, Models.Product>
                .NewConfig()
                .Map(dest => dest.Category,      src => src.Category.ToString())
                .Map(dest => dest.UnitOfMeasure, src => src.UnitOfMeasure.ToString());
        }
    }
}
