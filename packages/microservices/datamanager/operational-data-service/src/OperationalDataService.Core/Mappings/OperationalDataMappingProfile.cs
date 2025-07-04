using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Mappings;

public class OperationalDataMappingProfile : Profile
{
    public OperationalDataMappingProfile()
    {
        // Product Catalog Mappings
        CreateMap<ProductCatalog, ProductCatalogDto>().ReverseMap();
        
        // Route Configuration Mappings
        CreateMap<RouteConfiguration, OptimizedRoute>()
            .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.RouteId))
            .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin))
            .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination))
            .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
            .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom(src => src.EstimatedDuration))
            .ForMember(dest => dest.EstimatedCost, opt => opt.MapFrom(src => src.CostPerKm * src.Distance + src.TollCost))
            .ForMember(dest => dest.EstimatedFuelCost, opt => opt.MapFrom(src => src.CostPerKm * src.Distance))
            .ForMember(dest => dest.TollCost, opt => opt.MapFrom(src => src.TollCost))
            .ForMember(dest => dest.TrafficConditions, opt => opt.MapFrom(src => src.TrafficCondition))
            .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => src.Waypoints))
            .ForMember(dest => dest.Restrictions, opt => opt.MapFrom(src => src.Restrictions))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // Weighbridge Operation Mappings
        CreateMap<WeighbridgeOperation, WeighbridgeCapacity>()
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CurrentLoad, opt => opt.MapFrom(src => src.CurrentLoad))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxHourlyCapacity))
            .ForMember(dest => dest.HourlyCapacity, opt => opt.MapFrom(src => src.MaxHourlyCapacity))
            .ForMember(dest => dest.UtilizationRate, opt => opt.MapFrom(src => src.UtilizationRate))
            .ForMember(dest => dest.VehiclesInQueue, opt => opt.MapFrom(src => src.QueuedVehicles.Count))
            .ForMember(dest => dest.EstimatedWaitTime, opt => opt.MapFrom(src => TimeSpan.FromMinutes((double)src.EstimatedWaitTime)))
            .ForMember(dest => dest.NextAvailableSlot, opt => opt.MapFrom(src => src.NextAvailableSlot))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt));

        // Capacity Management Mappings
        CreateMap<CapacityManagement, WeighbridgeCapacity>()
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId))
            .ForMember(dest => dest.CurrentLoad, opt => opt.MapFrom(src => src.CurrentLoad))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxCapacity))
            .ForMember(dest => dest.HourlyCapacity, opt => opt.MapFrom(src => src.HourlyCapacity))
            .ForMember(dest => dest.UtilizationRate, opt => opt.MapFrom(src => src.UtilizationRate))
            .ForMember(dest => dest.VehiclesInQueue, opt => opt.MapFrom(src => src.VehiclesInQueue))
            .ForMember(dest => dest.EstimatedWaitTime, opt => opt.MapFrom(src => src.EstimatedWaitTime))
            .ForMember(dest => dest.AverageProcessingTime, opt => opt.MapFrom(src => src.AverageProcessingTime))
            .ForMember(dest => dest.NextAvailableSlot, opt => opt.MapFrom(src => src.NextAvailableSlot))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated));

        CreateMap<Entities.CapacityForecast, HourlyCapacityForecast>()
            .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.ForecastDate))
            .ForMember(dest => dest.PredictedLoad, opt => opt.MapFrom(src => src.PredictedLoad))
            .ForMember(dest => dest.PredictedUtilization, opt => opt.MapFrom(src => src.PredictedUtilization))
            .ForMember(dest => dest.PredictedWaitTime, opt => opt.MapFrom(src => src.PredictedWaitTime))
            .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.Confidence));

        // Operational Alert Mappings
        CreateMap<OperationalAlert, OperationalAlertSummary>()
            .ForMember(dest => dest.AlertId, opt => opt.MapFrom(src => src.AlertId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TriggeredAt, opt => opt.MapFrom(src => src.TriggeredAt))
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId));

        // Maintenance Schedule Mappings
        CreateMap<MaintenanceSchedule, UpcomingMaintenance>()
            .ForMember(dest => dest.MaintenanceId, opt => opt.MapFrom(src => src.MaintenanceId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId))
            .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.ScheduledDate))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority));

        CreateMap<MaintenanceSchedule, OverdueMaintenance>()
            .ForMember(dest => dest.MaintenanceId, opt => opt.MapFrom(src => src.MaintenanceId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId))
            .ForMember(dest => dest.ScheduledDate, opt => opt.MapFrom(src => src.ScheduledDate))
            .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src => (DateTime.UtcNow - src.ScheduledDate).Days))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority));

        // System Configuration Mappings
        CreateMap<SystemConfiguration, ConfigurationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ConfigurationId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        // Operational Schedule Mappings
        CreateMap<OperationalSchedule, ScheduleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.WeighbridgeId, opt => opt.MapFrom(src => src.WeighbridgeId));
    }
}

// Additional DTOs for mapping
public class ConfigurationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class ScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string WeighbridgeId { get; set; } = string.Empty;
}