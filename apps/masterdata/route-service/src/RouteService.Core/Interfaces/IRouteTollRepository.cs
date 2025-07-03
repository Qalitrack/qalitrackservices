using RouteService.Core.Entities;
using RouteService.Core.DTOs;

namespace RouteService.Core.Interfaces;

public interface IRouteTollRepository : IRepository<RouteToll>
{
    Task<IEnumerable<RouteToll>> GetTollsByRouteIdAsync(string routeId);
    Task<IEnumerable<RouteToll>> GetTollsByTypeAsync(TollType type);
    Task<decimal> CalculateTotalTollCostAsync(string routeId, VehicleSpecifications vehicle);
    Task<IEnumerable<RouteToll>> GetETCTollsAsync();
    Task<IEnumerable<RouteToll>> GetTollsWithAlternativeRoutesAsync();
    Task<IEnumerable<RouteToll>> GetTollsByPaymentMethodAsync(PaymentMethod paymentMethod);
    Task<IEnumerable<RouteToll>> GetTollsByDistanceRangeAsync(double startDistance, double endDistance);
    Task<RouteToll?> GetNearestTollAsync(double latitude, double longitude);
}