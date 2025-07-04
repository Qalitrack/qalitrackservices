using RouteService.Core.Entities;
using RouteService.Core.DTOs;

namespace RouteService.Core.Interfaces;

public interface IRouteRepository : IRepository<Route>
{
    Task<IEnumerable<Route>> GetRoutesBetweenAsync(string origin, string destination);
    Task<IEnumerable<Route>> GetActiveRoutesAsync();
    Task<IEnumerable<Route>> GetRoutesByTypeAsync(RouteType routeType);
    Task<IEnumerable<Route>> GetRoutesForVehicleAsync(VehicleSpecifications vehicle);
    Task<Route?> GetRouteByCodeAsync(string code);
    Task<IEnumerable<Route>> GetRoutesWithWaypointsAsync();
    Task<IEnumerable<Route>> GetRoutesWithConditionsAsync();
    Task<IEnumerable<Route>> GetRoutesWithRestrictionsAsync();
    Task<IEnumerable<Route>> SearchRoutesAsync(string searchTerm);
}