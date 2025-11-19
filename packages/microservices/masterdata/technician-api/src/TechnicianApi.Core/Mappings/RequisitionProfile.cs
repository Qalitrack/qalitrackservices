using AutoMapper;
using TechnicianApi.Core.DTOs.Requisition;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class RequisitionProfile : Profile
{
    public RequisitionProfile()
    {
        // Entity to ResponseDto
        CreateMap<Requisition, RequisitionResponseDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // CreateDto to Entity
        CreateMap<CreateRequisitionDto, Requisition>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<RequisitionType>(src.Type)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RequisitionStatus.Pending))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.TmReviewedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TmReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.TmComments, opt => opt.Ignore())
            .ForMember(dest => dest.CfoReviewedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CfoReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CfoComments, opt => opt.Ignore())
            .ForMember(dest => dest.VoucherNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ReferenceNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAt, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RejectedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        // UpdateDto to Entity
        CreateMap<UpdateRequisitionDto, Requisition>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? Enum.Parse<RequisitionStatus>(src.Status) : default(RequisitionStatus?)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
