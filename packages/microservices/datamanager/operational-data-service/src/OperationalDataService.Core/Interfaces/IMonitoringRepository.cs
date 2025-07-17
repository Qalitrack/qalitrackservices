using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IMonitoringRepository : IRepository<Monitoring>
{
    Task<List<Monitoring>> GetByOperationAsync(string operationId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Monitoring>> GetByProcessAsync(string processId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Monitoring>> GetByMetricNameAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Monitoring>> GetByTypeAsync(MonitoringType monitoringType, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Monitoring>> GetBySourceAsync(string source, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Monitoring>> GetActiveAlertsAsync();
    Task<List<Monitoring>> GetUnacknowledgedAlertsAsync();
    Task<List<Monitoring>> GetAlertsByLevelAsync(AlertLevel alertLevel);
    Task<List<Monitoring>> GetRecentMetricsAsync(int minutes);
    Task<List<Monitoring>> GetMetricsExceedingThresholdsAsync();
    Task<Monitoring?> GetLatestMetricAsync(string metricName);
    Task<List<Monitoring>> GetMetricsByDateRangeAsync(string metricName, DateTime fromDate, DateTime toDate);
    Task<(decimal min, decimal max, decimal avg)> GetMetricStatisticsAsync(string metricName, DateTime fromDate, DateTime toDate);
}