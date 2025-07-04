using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRouteConditionRepository : IRepository<RouteCondition>
{
    Task<IEnumerable<RouteCondition>> GetConditionsByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteCondition>> GetActiveConditionsAsync(string routeId);
    Task<IEnumerable<RouteCondition>> GetConditionsByTypeAsync(ConditionType type);
    Task<IEnumerable<RouteCondition>> GetConditionsByStatusAsync(ConditionStatus status);
    Task<IEnumerable<RouteCondition>> GetRecentConditionsAsync(DateTime since);
    Task<IEnumerable<RouteCondition>> GetConditionsByLocationAsync(double latitude, double longitude, double radiusKm);
    Task<IEnumerable<RouteCondition>> GetCriticalConditionsAsync();
    Task<int> GetActiveConditionCountAsync(string routeId);
    Task<RouteCondition?> GetMostRecentConditionAsync(string routeId);
}