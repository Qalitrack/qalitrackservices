using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Core.Interfaces;

public interface IRouteOptimizationService
{
    Task<OptimizedRoute> OptimizeRouteAsync(RouteOptimizationRequest request);
    Task<List<RouteRecommendation>> GetRouteRecommendationsAsync(string origin, string destination, string? organizationId = null);
    Task<RoutePerformanceMetricsDto> AnalyzeRoutePerformanceAsync(string routeId, TimeRange period);
    Task UpdateTrafficPatternsAsync(string routeId, TrafficData trafficData);
    Task<List<OptimizedRoute>> GetAlternativeRoutesAsync(RouteOptimizationRequest request, int maxAlternatives = 3);
    Task<OptimizedRoute?> GetRouteByIdAsync(string routeId);
    Task<List<RouteIssue>> GetActiveRouteIssuesAsync(string? routeId = null);
    Task<TrafficData> GetCurrentTrafficDataAsync(string routeId);
    Task<List<OptimizedRoute>> GetFavoriteRoutesAsync(string origin, string destination, string organizationId);
    Task<OptimizedRoute> SaveRouteAsync(OptimizedRoute route);
    Task<bool> DeleteRouteAsync(string routeId);
    Task<List<OptimizedRoute>> GetRouteHistoryAsync(string origin, string destination, DateTime fromDate, DateTime toDate);
    Task<RouteQuality> CalculateRouteQualityAsync(string routeId);
    Task ReportRouteIssueAsync(string routeId, RouteIssue issue);
}