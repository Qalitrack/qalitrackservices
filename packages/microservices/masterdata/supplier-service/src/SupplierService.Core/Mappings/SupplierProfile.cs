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
            .ForMember(dest => dest.SupplierType, opt => opt.MapFrom(src => src.SupplierType));

        CreateMap<Supplier, SupplierReadDto>()
            .ForMember(dest => dest.SupplierType, opt => opt.MapFrom(src => src.SupplierType))
            .ForMember(dest => dest.SupplierStatusValue, opt => opt.MapFrom(src => src.Status));

        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.ContactPerson, opt => opt.Ignore())
            .ForMember(dest => dest.TaxIdentificationNumber, opt => opt.MapFrom(src => src.TaxNumber))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.SupplierType))
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationDate, opt => opt.Ignore());

        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.ContactPerson, opt => opt.Ignore())
            .ForMember(dest => dest.TaxIdentificationNumber, opt => opt.MapFrom(src => src.TaxNumber))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.SupplierType))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Address mappings
        CreateMap<Address, AddressDto>();

        CreateMap<CreateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<UpdateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // SupplierProduct mappings
        CreateMap<SupplierProduct, SupplierProductDto>();

        CreateMap<CreateSupplierProductDto, SupplierProduct>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                Enum.Parse<ProductAvailabilityStatus>(src.Status, true)))
            .ForMember(dest => dest.LeadTimeDays, opt => opt.MapFrom(src => src.LeadTime))
            .ForMember(dest => dest.MinimumOrderQuantity, opt => opt.MapFrom(src => src.MinOrderQuantity))
            .ForMember(dest => dest.ProductName, opt => opt.Ignore());

        CreateMap<UpdateSupplierProductDto, SupplierProduct>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                src.Status != null ? Enum.Parse<ProductAvailabilityStatus>(src.Status, true) : (ProductAvailabilityStatus?)null))
            .ForMember(dest => dest.LeadTimeDays, opt => opt.MapFrom(src => src.LeadTime))
            .ForMember(dest => dest.MinimumOrderQuantity, opt => opt.MapFrom(src => src.MinOrderQuantity))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


        // SupplierPricing mappings
        CreateMap<SupplierPricing, SupplierPricingDto>();

        CreateMap<CreateSupplierPricingDto, SupplierPricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 
                Enum.Parse<PricingType>(src.Type, true)));

        CreateMap<UpdateSupplierPricingDto, SupplierPricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 
                src.Type != null ? Enum.Parse<PricingType>(src.Type, true) : (PricingType?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // SupplierPerformance mappings
        CreateMap<SupplierPerformance, SupplierPerformanceDto>();

        CreateMap<CreateSupplierPerformanceDto, SupplierPerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MetricType, opt => opt.MapFrom(src => 
                Enum.Parse<PerformanceMetricType>(src.MetricType, true)))
            .ForMember(dest => dest.Year, opt => opt.Ignore())
            .ForMember(dest => dest.Month, opt => opt.Ignore());

        CreateMap<UpdateSupplierPerformanceDto, SupplierPerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MetricType, opt => opt.MapFrom(src => 
                src.MetricType != null ? Enum.Parse<PerformanceMetricType>(src.MetricType, true) : (PerformanceMetricType?)null))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}