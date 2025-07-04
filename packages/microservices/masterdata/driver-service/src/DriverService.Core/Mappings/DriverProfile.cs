using AutoMapper;
using DriverService.Core.DTOs;
using DriverService.Core.Entities;

namespace DriverService.Core.Mappings;

public class DriverProfile : Profile
{
    public DriverProfile()
    {
        // Driver mappings
        CreateMap<Driver, DriverDto>()
            .ForMember(dest => dest.License, opt => opt.MapFrom(src => src.License))
            .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile));
        
        CreateMap<CreateDriverDto, Driver>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DriverStatus.Active));
        
        CreateMap<UpdateDriverDto, Driver>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // Driver License mappings
        CreateMap<DriverLicense, DriverLicenseDto>();
        CreateMap<CreateDriverLicenseDto, DriverLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
        
        CreateMap<UpdateDriverLicenseDto, DriverLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DriverId, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Profile mappings
        CreateMap<Entities.DriverProfile, DriverProfileDto>();
        CreateMap<CreateDriverProfileDto, Entities.DriverProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
        
        CreateMap<UpdateDriverProfileDto, Entities.DriverProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DriverId, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Document mappings
        CreateMap<DriverDocument, DriverDocumentDto>();
        CreateMap<CreateDriverDocumentDto, DriverDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
        
        CreateMap<UpdateDriverDocumentDto, DriverDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DriverId, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Violation mappings
        CreateMap<DriverViolation, DriverViolationDto>();
        CreateMap<CreateDriverViolationDto, DriverViolation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
        
        CreateMap<UpdateDriverViolationDto, DriverViolation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DriverId, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Training mappings
        CreateMap<DriverTraining, DriverTrainingDto>();
        CreateMap<CreateDriverTrainingDto, DriverTraining>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Medical mappings
        CreateMap<DriverMedical, DriverMedicalDto>();
        CreateMap<CreateDriverMedicalDto, DriverMedical>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());

        // Driver Performance mappings
        CreateMap<DriverPerformance, DriverPerformanceDto>();
        CreateMap<CreateDriverPerformanceDto, DriverPerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
    }
}