using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRouteWaypointRepository : IRepository<RouteWaypoint>
{
    Task<IEnumerable<RouteWaypoint>> GetWaypointsByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteWaypoint>> GetWaypointsByTypeAsync(WaypointType type);
    Task<RouteWaypoint?> GetNextWaypointAsync(string routeId, int currentSequence);
    Task<RouteWaypoint?> GetPreviousWaypointAsync(string routeId, int currentSequence);
    Task<IEnumerable<RouteWaypoint>> GetRequiredWaypointsAsync(string routeId);
    Task<int> GetMaxSequenceForRouteAsync(string routeId);
    Task ResequenceWaypointsAsync(string routeId);
}