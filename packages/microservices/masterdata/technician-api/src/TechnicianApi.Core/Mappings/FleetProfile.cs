using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class FleetProfile : Profile
{
    public FleetProfile()
    {
        // Material mappings
        CreateMap<Material, MaterialResponseDto>();
        CreateMap<CreateMaterialDto, Material>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.Ignore())
            .ForMember(dest => dest.Costs, opt => opt.Ignore())
            .ForMember(dest => dest.Trips, opt => opt.Ignore())
            .ForMember(dest => dest.TripMaterials, opt => opt.Ignore());

        // MaterialVariant mappings
        CreateMap<MaterialVariant, MaterialVariantResponseDto>();
        CreateMap<CreateMaterialVariantDto, MaterialVariant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Material, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.Ignore())
            .ForMember(dest => dest.Trips, opt => opt.Ignore())
            .ForMember(dest => dest.TripMaterials, opt => opt.Ignore());

        // MaterialCost mappings
        CreateMap<MaterialCost, MaterialCostResponseDto>();
        CreateMap<CreateMaterialCostDto, MaterialCost>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Synced, opt => opt.Ignore())
            .ForMember(dest => dest.Material, opt => opt.Ignore());

        // Truck mappings
        CreateMap<Truck, TruckResponseDto>();
        CreateMap<CreateTruckDto, Truck>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.Trips, opt => opt.Ignore())
            .ForMember(dest => dest.VehicleMileages, opt => opt.Ignore());

        // LicenseClass mappings
        CreateMap<LicenseClass, LicenseClassResponseDto>();
    }
}
