using AutoMapper;
using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class DriverMappingProfile : Profile
{
    public DriverMappingProfile()
    {
        // DriverProfile mappings
        CreateMap<DriverProfile, DriverProfileResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateDriverProfileDto, DriverProfile>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DriverProfileStatus.Draft))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseClasses, opt => opt.Ignore())
            .ForMember(dest => dest.Changes, opt => opt.Ignore())
            .ForMember(dest => dest.IsCurrent, opt => opt.Ignore())
            .ForMember(dest => dest.VersionNumber, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseExpiryDate, opt => opt.MapFrom(src =>
                src.LicenseExpiryDate.HasValue
                    ? DateTime.SpecifyKind(src.LicenseExpiryDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null));
        CreateMap<UpdateDriverProfileDto, DriverProfile>()
            .ForMember(dest => dest.LicenseExpiryDate, opt => opt.MapFrom(src =>
                src.LicenseExpiryDate.HasValue
                    ? DateTime.SpecifyKind(src.LicenseExpiryDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // DriverActivity mappings
        CreateMap<DriverActivity, DriverActivityResponseDto>()
            .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType.ToString()));
    }
}
