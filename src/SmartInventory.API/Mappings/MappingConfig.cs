using Mapster;
using SmartInventory.Application.Features.Products.Commands.CreateProduct;
using SmartInventory.Application.Features.Products.DTO;
using SmartInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using SmartInventory.Application.Features.Warehouses.DTO;
using SmartInventory.Contracts.Requests.Products;
using SmartInventory.Contracts.Requests.Warehouses;
using SmartInventory.Contracts.Responses.Products;
using SmartInventory.Contracts.Responses.Warehouses;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;

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

            TypeAdapterConfig<CreateProductRequest, CreateProductCommand>
                .NewConfig()
                .Map(dest => dest.Category,
                     src  => Enum.Parse<ProductCategory>(src.Category, ignoreCase: true))
                .Map(dest => dest.UnitOfMeasure,
                     src  => Enum.Parse<UnitOfMeasure>(src.UnitOfMeasure, ignoreCase: true));

            TypeAdapterConfig<ProductDto, ProductResponse>
                .NewConfig()
                .Map(dest => dest.Category,      src => src.Category.ToString())
                .Map(dest => dest.UnitOfMeasure, src => src.UnitOfMeasure.ToString());

            TypeAdapterConfig<CreateWarehouseRequest, CreateWarehouseCommand>
                .NewConfig()
                .Map(dest => dest.WarehouseType, src  => Enum.Parse<WarehouseType>(src.WarehouseType, ignoreCase: true));

            TypeAdapterConfig<Warehouse, WarehouseDto>
                .NewConfig()
                .Map(dest => dest.Street,     src => src.Address != null ? src.Address.Street     : string.Empty)
                .Map(dest => dest.City,       src => src.Address != null ? src.Address.City       : string.Empty)
                .Map(dest => dest.State,      src => src.Address != null ? src.Address.State      : string.Empty)
                .Map(dest => dest.PostalCode, src => src.Address != null ? src.Address.PostalCode : string.Empty)
                .Map(dest => dest.Country,    src => src.Address != null ? src.Address.Country    : string.Empty);
            TypeAdapterConfig<WarehouseDto, WarehouseResponse>
                .NewConfig()
                .Map(dest => dest.WarehouseType, src => src.WarehouseType.ToString());
        }
    }
}
