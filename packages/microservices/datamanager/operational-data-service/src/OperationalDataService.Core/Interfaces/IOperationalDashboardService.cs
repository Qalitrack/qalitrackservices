using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Core.Interfaces;

public interface IOperationalDashboardService
{
    Task<OperationalDashboardDto> GetDashboardDataAsync(string organizationId);
    Task<WeighbridgeStatusSummary> GetWeighbridgeStatusSummaryAsync(string organizationId);
    Task<CapacityUtilizationSummary> GetCapacityUtilizationAsync(string organizationId);
    Task<ProductSummary> GetActiveProductCountAsync(string organizationId);
    Task<RoutePerformanceSummary> GetRoutePerformanceSummaryAsync(string organizationId);
    Task<List<OperationalAlertSummary>> GetActiveAlertsAsync(string organizationId);
    Task<MaintenanceScheduleSummary> GetUpcomingMaintenanceAsync(string organizationId);
    Task<PerformanceMetricsSummary> GetOperationalMetricsAsync(string organizationId);
    Task<List<CapacityForecastSummary>> GetCapacityForecastsAsync(string organizationId);
    Task<SystemHealthSummary> GetSystemHealthAsync();
    Task RefreshDashboardDataAsync(string organizationId);
    Task<List<KpiMetric>> GetKpiMetricsAsync(string organizationId);
    Task<List<PerformanceTrend>> GetPerformanceTrendsAsync(string organizationId, TimeRange period);
}