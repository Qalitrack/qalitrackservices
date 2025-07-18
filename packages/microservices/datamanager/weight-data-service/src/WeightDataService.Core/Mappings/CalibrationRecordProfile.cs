using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class CalibrationRecordProfile : Profile
{
    public CalibrationRecordProfile()
    {
        CreateMap<CalibrationRecord, CalibrationRecordDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateCalibrationRecordDto, CalibrationRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CalibrationId, opt => opt.Ignore())
            .ForMember(dest => dest.CalibrationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Drift, opt => opt.Ignore())
            .ForMember(dest => dest.DriftPercentage, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.NextCalibrationDue, opt => opt.Ignore())
            .ForMember(dest => dest.CertificateNumber, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore());
    }
}