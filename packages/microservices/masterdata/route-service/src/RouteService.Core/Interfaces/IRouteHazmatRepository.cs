using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRouteHazmatRepository : IRepository<RouteHazmat>
{
    Task<IEnumerable<RouteHazmat>> GetHazmatByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteHazmat>> GetHazmatByClassAsync(string hazmatClass);
    Task<IEnumerable<RouteHazmat>> GetProhibitedHazmatAsync(string routeId);
    Task<IEnumerable<RouteHazmat>> GetPermitRequiredHazmatAsync(string routeId);
    Task<IEnumerable<RouteHazmat>> GetEscortRequiredHazmatAsync(string routeId);
    Task<IEnumerable<RouteHazmat>> GetHazmatByRestrictionTypeAsync(HazmatRestrictionType restrictionType);
    Task<bool> IsHazmatAllowedAsync(string routeId, string hazmatClass, DateTime checkTime);
    Task<IEnumerable<RouteHazmat>> GetTimeRestrictedHazmatAsync(TimeOnly checkTime);
    Task<IEnumerable<RouteHazmat>> GetHazmatWithQuantityLimitsAsync(string routeId);
    Task<IEnumerable<RouteHazmat>> GetHazmatByLocationAsync(double latitude, double longitude);
}