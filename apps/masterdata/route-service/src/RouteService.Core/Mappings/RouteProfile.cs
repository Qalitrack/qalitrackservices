using AutoMapper;
using RouteService.Core.DTOs;
using RouteService.Core.Entities;

namespace RouteService.Core.Mappings;

public class RouteProfile : Profile
{
    public RouteProfile()
    {
        // Route mappings
        CreateMap<Route, RouteDto>();
        CreateMap<CreateRouteRequest, Route>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RouteStatus.Active))
            .ForMember(dest => dest.Waypoints, opt => opt.Ignore())
            .ForMember(dest => dest.Restrictions, opt => opt.Ignore())
            .ForMember(dest => dest.Conditions, opt => opt.Ignore())
            .ForMember(dest => dest.Tolls, opt => opt.Ignore())
            .ForMember(dest => dest.Performance, opt => opt.Ignore())
            .ForMember(dest => dest.HazmatRestrictions, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        // RouteWaypoint mappings
        CreateMap<RouteWaypoint, RouteWaypointDto>();
        CreateMap<CreateRouteWaypointRequest, RouteWaypoint>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RouteRestriction mappings
        CreateMap<RouteRestriction, RouteRestrictionDto>();
        CreateMap<CreateRouteRestrictionRequest, RouteRestriction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RouteCondition mappings
        CreateMap<RouteCondition, RouteConditionDto>();
        CreateMap<CreateRouteConditionRequest, RouteCondition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RouteToll mappings
        CreateMap<RouteToll, RouteTollDto>();
        CreateMap<CreateRouteTollRequest, RouteToll>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RoutePerformance mappings
        CreateMap<RoutePerformance, RoutePerformanceDto>();
        CreateMap<CreateRoutePerformanceRequest, RoutePerformance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RouteHazmat mappings
        CreateMap<RouteHazmat, RouteHazmatDto>();
        CreateMap<CreateRouteHazmatRequest, RouteHazmat>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore());

        // RouteSchedule mappings
        CreateMap<RouteSchedule, RouteScheduleDto>();
        CreateMap<CreateRouteScheduleRequest, RouteSchedule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RouteId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Route, opt => opt.Ignore());
    }
}