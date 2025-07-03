using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class WeightMeasurementProfile : Profile
{
    public WeightMeasurementProfile()
    {
        CreateMap<WeightMeasurement, WeightMeasurementDto>()
            .ForMember(dest => dest.Corrections, opt => opt.MapFrom(src => src.Corrections));

        CreateMap<CreateWeightMeasurementDto, WeightMeasurement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MeasurementStatus.Pending))
            .ForMember(dest => dest.MeasurementDateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.NetWeight, opt => opt.MapFrom(src => CalculateNetWeight(src.Weight, src.TareWeight)))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Corrections, opt => opt.Ignore());

        CreateMap<UpdateWeightMeasurementDto, WeightMeasurement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.VehicleRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.MeasurementDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Corrections, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }

    private static decimal? CalculateNetWeight(decimal weight, decimal? tareWeight)
    {
        return tareWeight.HasValue ? weight - tareWeight.Value : null;
    }
}