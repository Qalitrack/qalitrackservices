using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class RealTimeSessionProfile : Profile
{
    public RealTimeSessionProfile()
    {
        CreateMap<RealTimeSession, RealTimeSessionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateRealTimeSessionDto, RealTimeSession>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SessionId, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StreamingStatus.Active))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Measurements, opt => opt.Ignore());
    }
}