using RouteService.Core.Entities;
using RouteService.Core.DTOs;

namespace RouteService.Core.Interfaces;

public interface IRouteRestrictionRepository : IRepository<RouteRestriction>
{
    Task<IEnumerable<RouteRestriction>> GetRestrictionsByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteRestriction>> GetActiveRestrictionsAsync(string routeId);
    Task<IEnumerable<RouteRestriction>> GetRestrictionsByTypeAsync(RestrictionType type);
    Task<IEnumerable<RouteRestriction>> GetRestrictionsBySeverityAsync(RestrictionSeverity severity);
    Task<IEnumerable<RouteRestriction>> GetTimeBasedRestrictionsAsync(DateTime checkTime);
    Task<IEnumerable<RouteRestriction>> GetVehicleRestrictionsAsync(VehicleSpecifications vehicle);
    Task<IEnumerable<RouteRestriction>> GetHazmatRestrictionsAsync(List<string> hazmatClasses);
    Task<bool> IsRouteRestrictedForVehicleAsync(string routeId, VehicleSpecifications vehicle, DateTime checkTime);
}