using AutoMapper;
using TechnicianApi.Core.DTOs.Feedback;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class FeedbackProfile : Profile
{
    public FeedbackProfile()
    {
        // Feedback mappings
        CreateMap<Feedback, FeedbackResponseDto>()
            .ForMember(dest => dest.FeedbackType, opt => opt.MapFrom(src => src.FeedbackType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateFeedbackDto, Feedback>()
            .ForMember(dest => dest.FeedbackType, opt => opt.MapFrom(src => Enum.Parse<FeedbackType>(src.FeedbackType)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FeedbackStatus.Pending))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.AdminResponse, opt => opt.Ignore())
            .ForMember(dest => dest.RespondedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseDate, opt => opt.Ignore());

        CreateMap<UpdateFeedbackDto, Feedback>()
            .ForMember(dest => dest.FeedbackType, opt => opt.MapFrom(src => Enum.Parse<FeedbackType>(src.FeedbackType)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
