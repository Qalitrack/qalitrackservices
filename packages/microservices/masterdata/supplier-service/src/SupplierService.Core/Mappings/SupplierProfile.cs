using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;

namespace SupplierService.Core.Mappings;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        // Supplier mappings
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<Supplier, SupplierReadDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count))
            .ForMember(dest => dest.AveragePerformanceScore, opt => opt.MapFrom(src => 
                src.PerformanceMetrics.Any() ? src.PerformanceMetrics.Average(p => p.Score) : 0));

        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<SupplierStatus>(src.Type, true)))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<SupplierType>(src.Type, true)));

        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? Enum.Parse<SupplierStatus>(src.Status, true) : (SupplierStatus?)null))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type != null ? Enum.Parse<SupplierType>(src.Type, true) : (SupplierType?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Address mappings
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<CreateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<AddressType>(src.Type, true)));

        CreateMap<UpdateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type != null ? Enum.Parse<AddressType>(src.Type, true) : (AddressType?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // SupplierProduct mappings
        CreateMap<SupplierProduct, SupplierProductDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateSupplierProductDto, SupplierProduct>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ProductAvailabilityStatus>(src.Status, true)));

        CreateMap<UpdateSupplierProductDto, SupplierProduct>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? Enum.Parse<ProductAvailabilityStatus>(src.Status, true) : (ProductAvailabilityStatus?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // SupplierPricing mappings
        CreateMap<SupplierPricing, SupplierPricingDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<CreateSupplierPricingDto, SupplierPricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<PricingType>(src.Type, true)));

        CreateMap<UpdateSupplierPricingDto, SupplierPricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type != null ? Enum.Parse<PricingType>(src.Type, true) : (PricingType?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // SupplierPerformance mappings
        CreateMap<SupplierPerformance, SupplierPerformanceDto>()
            .ForMember(dest => dest.MetricType, opt => opt.MapFrom(src => src.MetricType.ToString()))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString()));

        CreateMap<CreateSupplierPerformanceDto, SupplierPerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MetricType, opt => opt.MapFrom(src => Enum.Parse<PerformanceMetricType>(src.MetricType, true)))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => Enum.Parse<PerformancePeriod>(src.Period, true)));

        CreateMap<UpdateSupplierPerformanceDto, SupplierPerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MetricType, opt => opt.MapFrom(src => src.MetricType != null ? Enum.Parse<PerformanceMetricType>(src.MetricType, true) : (PerformanceMetricType?)null))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period != null ? Enum.Parse<PerformancePeriod>(src.Period, true) : (PerformancePeriod?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}