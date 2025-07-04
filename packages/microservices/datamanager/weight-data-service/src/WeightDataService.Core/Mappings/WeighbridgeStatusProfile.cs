using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class WeighbridgeStatusProfile : Profile
{
    public WeighbridgeStatusProfile()
    {
        CreateMap<WeighbridgeStatus, WeighbridgeStatusDto>();

        CreateMap<CreateWeighbridgeStatusDto, WeighbridgeStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MaintenanceStatus.Active))
            .ForMember(dest => dest.CurrentWeight, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.MaintenanceNotes, opt => opt.Ignore());

        CreateMap<UpdateWeighbridgeStatusDto, WeighbridgeStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.MaxCapacity, opt => opt.Ignore())
            .ForMember(dest => dest.MinCapacity, opt => opt.Ignore())
            .ForMember(dest => dest.LastCalibrationDate, opt => opt.Ignore())
            .ForMember(dest => dest.SerialNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Manufacturer, opt => opt.Ignore())
            .ForMember(dest => dest.Model, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}