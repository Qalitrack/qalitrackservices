using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;

namespace ProductService.Core.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Product mappings
        CreateMap<ProductService.Core.Entities.Product, ProductReadDto>();
        CreateMap<CreateProductDto, ProductService.Core.Entities.Product>();
        CreateMap<UpdateProductDto, ProductService.Core.Entities.Product>();
        CreateMap<ProductService.Core.Entities.Product, ProductDto>().ReverseMap();

        // Category mappings
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CategoryReadDto>();

        // Pricing mappings
        CreateMap<Pricing, PricingDto>().ReverseMap();
        CreateMap<Pricing, PricingReadDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Strategy, opt => opt.MapFrom(src => src.Strategy.ToString()));

        // Specification mappings
        CreateMap<Specification, SpecificationDto>().ReverseMap();
        CreateMap<Specification, SpecificationReadDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}