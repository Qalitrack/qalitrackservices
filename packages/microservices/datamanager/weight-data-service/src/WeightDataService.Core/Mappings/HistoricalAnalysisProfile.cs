using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class HistoricalAnalysisProfile : Profile
{
    public HistoricalAnalysisProfile()
    {
        CreateMap<HistoricalAnalysis, HistoricalAnalysisDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<CreateHistoricalAnalysisDto, HistoricalAnalysis>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AnalysisId, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<AnalysisType>(src.Type)))
            .ForMember(dest => dest.TotalWeight, opt => opt.Ignore())
            .ForMember(dest => dest.AverageWeight, opt => opt.Ignore())
            .ForMember(dest => dest.MinWeight, opt => opt.Ignore())
            .ForMember(dest => dest.MaxWeight, opt => opt.Ignore())
            .ForMember(dest => dest.StandardDeviation, opt => opt.Ignore())
            .ForMember(dest => dest.TrendSlope, opt => opt.Ignore())
            .ForMember(dest => dest.TrendR2, opt => opt.Ignore())
            .ForMember(dest => dest.MeasurementCount, opt => opt.Ignore())
            .ForMember(dest => dest.TrendCategory, opt => opt.Ignore())
            .ForMember(dest => dest.AnomaliesDetected, opt => opt.Ignore())
            .ForMember(dest => dest.AnalysisDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore());
    }
}