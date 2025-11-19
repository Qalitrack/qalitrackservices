using AutoMapper;
using TechnicianApi.Core.DTOs.ServiceReport;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class ServiceReportProfile : Profile
{
    public ServiceReportProfile()
    {
        // Entity to ResponseDto
        CreateMap<ServiceReport, ServiceReportResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // CreateDto to Entity
        CreateMap<CreateServiceReportDto, ServiceReport>()
            .ForMember(dest => dest.TotalFieldJobMinutes, opt => opt.Ignore()) // Auto-calculated from check-ins
            .ForMember(dest => dest.StartDay, opt => opt.Ignore()) // Auto-calculated from check-ins
            .ForMember(dest => dest.EndDay, opt => opt.Ignore()) // Auto-calculated from check-ins
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ServiceReportStatus.Draft))
            .ForMember(dest => dest.SignedAt, opt => opt.MapFrom(src => src.SignatureData != null ? DateTime.UtcNow : (DateTime?)null))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        // UpdateDto to Entity
        CreateMap<UpdateServiceReportDto, ServiceReport>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? Enum.Parse<ServiceReportStatus>(src.Status) : default(ServiceReportStatus?)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
