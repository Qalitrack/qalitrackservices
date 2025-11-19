using AutoMapper;
using TechnicianApi.Core.DTOs.CheckIn;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class CheckInProfile : Profile
{
    public CheckInProfile()
    {
        // Entity to ResponseDto
        CreateMap<CheckIn, CheckInResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // CreateDto to Entity
        CreateMap<CreateCheckInDto, CheckIn>()
            .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CheckInStatus.OnSite))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());
    }
}
