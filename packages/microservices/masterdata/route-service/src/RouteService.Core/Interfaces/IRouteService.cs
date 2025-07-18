using RouteService.Core.DTOs;
using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRouteService
{
    // Route Management
    Task<RouteDto> CreateRouteAsync(CreateRouteRequest request);
    Task<RouteDto?> GetRouteAsync(string id);
    Task<RouteDto?> GetRouteByCodeAsync(string code);
    Task<IEnumerable<RouteDto>> GetAllRoutesAsync();
    Task<IEnumerable<RouteDto>> GetActiveRoutesAsync();
    Task<RouteDto> UpdateRouteAsync(string id, UpdateRouteRequest request);
    Task<bool> DeleteRouteAsync(string id);
    Task<IEnumerable<RouteDto>> SearchRoutesAsync(string searchTerm);
    Task<IEnumerable<RouteDto>> GetAvailableRoutesAsync(string origin, string destination, VehicleSpecifications vehicle);

    // Waypoint Management
    Task<RouteWaypointDto> AddWaypointAsync(string routeId, CreateRouteWaypointRequest request);
    Task<IEnumerable<RouteWaypointDto>> GetWaypointsAsync(string routeId);
    Task<RouteWaypointDto> UpdateWaypointAsync(string routeId, string waypointId, UpdateRouteWaypointRequest request);
    Task<bool> DeleteWaypointAsync(string routeId, string waypointId);
    Task ResequenceWaypointsAsync(string routeId);

    // Restriction Management
    Task<RouteRestrictionDto> AddRestrictionAsync(string routeId, CreateRouteRestrictionRequest request);
    Task<IEnumerable<RouteRestrictionDto>> GetRestrictionsAsync(string routeId);
    Task<IEnumerable<RouteRestrictionDto>> GetActiveRestrictionsAsync(string routeId);
    Task<RouteRestrictionDto> UpdateRestrictionAsync(string routeId, string restrictionId, UpdateRouteRestrictionRequest request);
    Task<bool> DeleteRestrictionAsync(string routeId, string restrictionId);
    Task<bool> IsVehicleAllowedAsync(string routeId, VehicleSpecifications vehicle, DateTime checkTime);

    // Condition Management
    Task<RouteConditionDto> ReportConditionAsync(string routeId, CreateRouteConditionRequest request);
    Task<IEnumerable<RouteConditionDto>> GetConditionsAsync(string routeId);
    Task<IEnumerable<RouteConditionDto>> GetActiveConditionsAsync(string routeId);
    Task<RouteConditionDto> UpdateConditionAsync(string routeId, string conditionId, UpdateRouteConditionRequest request);
    Task<bool> ResolveConditionAsync(string routeId, string conditionId);

    // Toll Management
    Task<RouteTollDto> AddTollAsync(string routeId, CreateRouteTollRequest request);
    Task<IEnumerable<RouteTollDto>> GetTollsAsync(string routeId);
    Task<decimal> CalculateTollCostAsync(string routeId, VehicleSpecifications vehicle);
    Task<RouteTollDto> UpdateTollAsync(string routeId, string tollId, UpdateRouteTollRequest request);
    Task<bool> DeleteTollAsync(string routeId, string tollId);

    // Performance Tracking
    Task<RoutePerformanceDto> RecordPerformanceAsync(string routeId, CreateRoutePerformanceRequest request);
    Task<IEnumerable<RoutePerformanceDto>> GetPerformanceHistoryAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<RoutePerformanceDto?> GetLatestPerformanceAsync(string routeId);
    Task<RoutePerformanceAnalytics> GetPerformanceAnalyticsAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);

    // Hazmat Management
    Task<RouteHazmatDto> AddHazmatRestrictionAsync(string routeId, CreateRouteHazmatRequest request);
    Task<IEnumerable<RouteHazmatDto>> GetHazmatRestrictionsAsync(string routeId);
    Task<bool> IsHazmatAllowedAsync(string routeId, string hazmatClass, DateTime checkTime);
    Task<RouteHazmatDto> UpdateHazmatRestrictionAsync(string routeId, string hazmatId, UpdateRouteHazmatRequest request);
    Task<bool> DeleteHazmatRestrictionAsync(string routeId, string hazmatId);

    // Schedule Management
    Task<RouteScheduleDto> CreateScheduleAsync(string routeId, CreateRouteScheduleRequest request);
    Task<IEnumerable<RouteScheduleDto>> GetSchedulesAsync(string routeId);
    Task<IEnumerable<RouteScheduleDto>> GetActiveSchedulesAsync(string routeId);
    Task<RouteScheduleDto?> GetNextScheduleAsync(string routeId, DateTime fromTime);
    Task<RouteScheduleDto> UpdateScheduleAsync(string routeId, string scheduleId, UpdateRouteScheduleRequest request);
    Task<bool> DeleteScheduleAsync(string routeId, string scheduleId);

    // Mapping and Traffic Integration
    Task<RouteMapDto?> GetRouteMapAsync(string routeId);
    Task<RouteTrafficDto?> GetRouteTrafficAsync(string routeId);
    Task<RouteOptimizationDto> OptimizeRouteAsync(string routeId, RouteOptimizationRequestDto request);
}

public class RoutePerformanceAnalytics
{
    public double AverageSpeed { get; set; }
    public double AverageTravelTime { get; set; }
    public double AverageFuelConsumption { get; set; }
    public double OnTimePerformance { get; set; }
    public int TotalIncidents { get; set; }
    public decimal AverageTollCosts { get; set; }
    public double AverageDelayTime { get; set; }
    public int TotalTrips { get; set; }
    public DateTime AnalyticsPeriodStart { get; set; }
    public DateTime AnalyticsPeriodEnd { get; set; }
}