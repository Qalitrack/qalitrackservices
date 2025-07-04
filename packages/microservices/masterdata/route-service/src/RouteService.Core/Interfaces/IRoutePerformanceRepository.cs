using RouteService.Core.Entities;

namespace RouteService.Core.Interfaces;

public interface IRoutePerformanceRepository : IRepository<RoutePerformance>
{
    Task<IEnumerable<RoutePerformance>> GetPerformanceByRouteIdAsync(string routeId);
    Task<IEnumerable<RoutePerformance>> GetPerformanceByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<RoutePerformance?> GetLatestPerformanceAsync(string routeId);
    Task<IEnumerable<RoutePerformance>> GetPerformanceByVehicleTypeAsync(string vehicleType);
    Task<IEnumerable<RoutePerformance>> GetPerformanceByDriverAsync(string driverId);
    Task<IEnumerable<RoutePerformance>> GetPerformanceByCompanyAsync(string companyId);
    Task<double> GetAverageSpeedAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<double> GetAverageTravelTimeAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<double> GetAverageFuelConsumptionAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<double> GetOnTimePerformanceAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetTotalIncidentsAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetAverageTollCostsAsync(string routeId, DateTime? startDate = null, DateTime? endDate = null);
}