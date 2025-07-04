using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;

namespace ProductService.Core.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

        CreateMap<RegisterProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ProductStatus>(src.Status)));

        // Category mappings
        CreateMap<ProductCategory, ProductCategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

        CreateMap<CreateProductCategoryRequest, ProductCategory>();

        // Specification mappings
        CreateMap<ProductSpecification, ProductSpecificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<ProductSpecificationDto, ProductSpecification>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<SpecificationType>(src.Type)));

        // Pricing mappings
        CreateMap<ProductPricing, ProductPricingDto>()
            .ForMember(dest => dest.PricingType, opt => opt.MapFrom(src => src.PricingType.ToString()));

        CreateMap<ProductPricingDto, ProductPricing>()
            .ForMember(dest => dest.PricingType, opt => opt.MapFrom(src => Enum.Parse<PricingType>(src.PricingType)));

        // Compliance mappings
        CreateMap<ProductCompliance, ComplianceRequirementDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ComplianceRequirementDto, ProductCompliance>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ComplianceStatus>(src.Status)));
    }
}