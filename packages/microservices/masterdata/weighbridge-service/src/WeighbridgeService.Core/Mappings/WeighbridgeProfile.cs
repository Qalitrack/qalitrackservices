using AutoMapper;
using WeighbridgeService.Core.DTOs;
using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.Mappings;

public class WeighbridgeProfile : Profile
{
    public WeighbridgeProfile()
    {
        // Weighbridge mappings
        CreateMap<Weighbridge, WeighbridgeDto>()
            .ForMember(dest => dest.WeighbridgeLocation, opt => opt.MapFrom(src => src.WeighbridgeLocation))
            .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src => src.Configuration))
            .ForMember(dest => dest.CurrentCapacity, opt => opt.MapFrom(src => src.CurrentCapacity))
            .ForMember(dest => dest.Operators, opt => opt.MapFrom(src => src.Operators))
            .ForMember(dest => dest.RecentCalibrations, opt => opt.MapFrom(src => src.CalibrationHistory.Take(5)))
            .ForMember(dest => dest.UpcomingMaintenance, opt => opt.MapFrom(src => src.MaintenanceSchedules.Where(m => m.ScheduledDate >= DateTime.Now).Take(5)));

        CreateMap<RegisterWeighbridgeRequest, Weighbridge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastCalibrationDate, opt => opt.MapFrom(src => src.CalibrationDate));

        CreateMap<UpdateWeighbridgeRequest, Weighbridge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeLocation mappings
        CreateMap<WeighbridgeLocation, WeighbridgeLocationDto>();
        CreateMap<CreateWeighbridgeLocationRequest, WeighbridgeLocation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeConfiguration mappings
        CreateMap<WeighbridgeConfiguration, WeighbridgeConfigurationDto>();
        CreateMap<UpdateWeighbridgeConfigurationRequest, WeighbridgeConfiguration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeCalibration mappings
        CreateMap<WeighbridgeCalibration, WeighbridgeCalibrationDto>();
        CreateMap<ScheduleCalibrationRequest, WeighbridgeCalibration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CalibrationStatus.Scheduled))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<RecordCalibrationRequest, WeighbridgeCalibration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CalibrationStatus.Completed))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeMaintenance mappings
        CreateMap<WeighbridgeMaintenance, WeighbridgeMaintenanceDto>();
        CreateMap<ScheduleMaintenanceRequest, WeighbridgeMaintenance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MaintenanceStatus.Scheduled))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateMaintenanceRequest, WeighbridgeMaintenance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeOperator mappings
        CreateMap<WeighbridgeOperator, WeighbridgeOperatorDto>();
        CreateMap<AssignOperatorRequest, WeighbridgeOperator>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OperatorStatus.Active))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // WeighbridgeSchedule mappings
        CreateMap<WeighbridgeSchedule, WeighbridgeScheduleDto>();

        // WeighbridgeCapacity mappings
        CreateMap<WeighbridgeCapacity, WeighbridgeCapacityDto>();
        CreateMap<UpdateCapacityRequest, WeighbridgeCapacity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeId, opt => opt.Ignore())
            .ForMember(dest => dest.MaxCapacity, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}