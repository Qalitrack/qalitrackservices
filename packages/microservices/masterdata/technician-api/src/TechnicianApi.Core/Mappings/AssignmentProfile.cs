using AutoMapper;
using TechnicianApi.Core.DTOs.Assignment;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        // Entity to ResponseDto
        CreateMap<Assignment, AssignmentResponseDto>()
            .ForMember(dest => dest.TechnicianIds, opt => opt.MapFrom(src => 
                src.TechnicianIds != null ? src.TechnicianIds.ToList() : new List<string>()))  // ✅ FIX HERE
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Rest of your mappings...
        CreateMap<CreateAssignmentDto, Assignment>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<AssignmentPriority>(src.Priority)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AssignmentStatus.Pending))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.AcceptedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIn, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceReport, opt => opt.Ignore())
            .ForMember(dest => dest.Requisitions, opt => opt.Ignore())
            .ForMember(dest => dest.PettyCashAdvanceForms, opt => opt.Ignore())
            .ForMember(dest => dest.AdvanceReturnForms, opt => opt.Ignore())
            .ForMember(dest => dest.PerDiemReturnForms, opt => opt.Ignore())
            .ForMember(dest => dest.Claims, opt => opt.Ignore())
            .ForMember(dest => dest.Refunds, opt => opt.Ignore())
            .ForMember(dest => dest.BalanceSummary, opt => opt.Ignore());

        CreateMap<UpdateAssignmentDto, Assignment>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => 
                src.Priority != null ? Enum.Parse<AssignmentPriority>(src.Priority) : default(AssignmentPriority?)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                src.Status != null ? Enum.Parse<AssignmentStatus>(src.Status) : default(AssignmentStatus?)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}