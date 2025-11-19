using AutoMapper;
using TechnicianApi.Core.DTOs.DailySummary;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class DailySummaryProfile : Profile
{
    public DailySummaryProfile()
    {
        // Entity to ResponseDto
        CreateMap<DailySummary, DailySummaryResponseDto>()
            .ForMember(dest => dest.AlertLevel, opt => opt.MapFrom(src => src.AlertLevel.ToString()));
    }
}
