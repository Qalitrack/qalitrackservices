using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Core.Interfaces;

public interface ICapacityManagementService
{
    Task<WeighbridgeCapacity> GetCurrentCapacityAsync(string weighbridgeId);
    Task<List<WeighbridgeAvailability>> GetAvailableWeighbridgesAsync(DateTime requestedTime, string organizationId);
    Task<CapacityForecast> ForecastCapacityAsync(string weighbridgeId, int forecastHours);
    Task<LoadBalancingResult> BalanceLoadAsync(List<string> weighbridgeIds, string organizationId);
    Task<List<WeighbridgeCapacity>> GetAllCapacitiesAsync(string organizationId);
    Task<CapacityAnalytics> GetCapacityAnalyticsAsync(string weighbridgeId, TimeRange period);
    Task UpdateCapacityAsync(string weighbridgeId, int currentLoad, decimal utilizationRate);
    Task<List<CapacityBottleneck>> IdentifyBottlenecksAsync(string organizationId);
    Task<List<CapacityRecommendation>> GetCapacityRecommendationsAsync(string weighbridgeId);
    Task<WeighbridgeAssignment> AssignOptimalWeighbridgeAsync(string transactionId, List<string> availableWeighbridges);
    Task<bool> ReserveCapacityAsync(string weighbridgeId, DateTime startTime, TimeSpan duration);
    Task<bool> ReleaseCapacityAsync(string weighbridgeId, DateTime startTime);
    Task<List<HourlyCapacityForecast>> GetDetailedForecastAsync(string weighbridgeId, DateTime date);
    Task RecalculateCapacityMetricsAsync(string weighbridgeId);
}