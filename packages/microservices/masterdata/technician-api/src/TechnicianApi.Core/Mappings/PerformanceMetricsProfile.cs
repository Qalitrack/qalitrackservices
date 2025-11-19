using AutoMapper;
using TechnicianApi.Core.DTOs.PerformanceMetrics;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class PerformanceMetricsProfile : Profile
{
    public PerformanceMetricsProfile()
    {
        // Entity to ResponseDto
        CreateMap<PerformanceMetrics, PerformanceMetricsResponseDto>()
            .ForMember(dest => dest.AlertLevel, opt => opt.MapFrom(src => src.AlertLevel.ToString()));
    }
}
