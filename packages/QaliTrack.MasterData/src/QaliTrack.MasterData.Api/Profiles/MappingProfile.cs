using AutoMapper;
using QaliTrack.MasterData.Core.Modules.Vehicle.Entities;
using QaliTrack.MasterData.Core.Modules.Driver.Entities;
using QaliTrack.MasterData.Core.Modules.SiteManagement.Entities;
using QaliTrack.MasterData.Core.Modules.SiteManagement.DTOs;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.Entities;
using QaliTrack.MasterData.Core.Modules.BusinessEntities.DTOs;

namespace QaliTrack.MasterData.Api.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // SiteManagement mappings
        CreateMap<Zone, ZoneDto>();
        CreateMap<CreateZoneDto, Zone>();
        CreateMap<UpdateZoneDto, Zone>();
        
        CreateMap<LocationType, LocationTypeDto>();
        CreateMap<CreateLocationTypeDto, LocationType>();
        CreateMap<UpdateLocationTypeDto, LocationType>();
        
        CreateMap<Site, SiteDto>();
        CreateMap<CreateSiteDto, Site>();
        CreateMap<UpdateSiteDto, Site>();

        // BusinessEntity mappings
        CreateMap<BusinessEntity, BusinessEntitySummaryDto>();
        CreateMap<BusinessEntity, BusinessEntityDetailDto>();
        CreateMap<CreateBusinessEntityDto, BusinessEntity>();
        CreateMap<UpdateBusinessEntityDto, BusinessEntity>();

        // Vehicle mappings
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Make} {src.Model} - {src.NumberPlate}"))
            .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.NumberPlate));
        CreateMap<CreateVehicleDto, Vehicle>()
            .ForMember(dest => dest.NumberPlate, opt => opt.MapFrom(src => src.RegistrationNumber));
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForMember(dest => dest.NumberPlate, opt => opt.MapFrom(src => src.RegistrationNumber));

        // Driver mappings
        CreateMap<Driver, DriverDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        CreateMap<CreateDriverDto, Driver>();
        CreateMap<UpdateDriverDto, Driver>();
    }
}

// DTOs for Vehicle
public class VehicleDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal MaxLegalLoad { get; set; }
    public decimal MaxSafeLoad { get; set; }
    public decimal TareWeight { get; set; }
    public bool HasContainer { get; set; }
    public string? ContainerType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public string? Notes { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateVehicleDto
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal MaxLegalLoad { get; set; }
    public decimal MaxSafeLoad { get; set; }
    public decimal TareWeight { get; set; }
    public bool HasContainer { get; set; } = false;
    public string? ContainerType { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsBlocked { get; set; } = false;
    public string? BlockedReason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVehicleDto
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public decimal MaxLegalLoad { get; set; }
    public decimal MaxSafeLoad { get; set; }
    public decimal TareWeight { get; set; }
    public bool HasContainer { get; set; }
    public string? ContainerType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public string? Notes { get; set; }
}

// DTOs for Driver
public class DriverDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? EmployeeId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDriverDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? EmployeeId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class UpdateDriverDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? EmployeeId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}