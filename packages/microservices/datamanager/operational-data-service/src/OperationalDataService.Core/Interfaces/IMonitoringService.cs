using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IMonitoringService
{
    // Metric Collection
    Task<MonitoringDto> RecordMetricAsync(RecordMetricRequest request);
    Task<List<MonitoringDto>> RecordMetricBatchAsync(List<RecordMetricRequest> requests);
    Task<MonitoringDto?> GetMetricAsync(string metricId);
    Task<bool> DeleteMetricAsync(string metricId);
    
    // Metric Querying
    Task<List<MonitoringDto>> GetMetricsByOperationAsync(string operationId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<MonitoringDto>> GetMetricsByProcessAsync(string processId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<MonitoringDto>> GetMetricsByNameAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<MonitoringDto>> GetMetricsByTypeAsync(MonitoringType monitoringType, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<MonitoringDto>> GetMetricsBySourceAsync(string source, DateTime? fromDate = null, DateTime? toDate = null);
    
    // Real-time Monitoring
    Task<List<MonitoringDto>> GetActiveMetricsAsync();
    Task<List<MonitoringDto>> GetRecentMetricsAsync(int minutes = 60);
    Task<SystemHealthDto> GetSystemHealthAsync();
    Task<OperationHealthDto> GetOperationHealthAsync(string operationId);
    Task<ProcessHealthDto> GetProcessHealthAsync(string processId);
    
    // Alerting and Notifications
    Task<List<MonitoringDto>> GetActiveAlertsAsync();
    Task<List<MonitoringDto>> GetUnacknowledgedAlertsAsync();
    Task<bool> AcknowledgeAlertAsync(string metricId, string userId);
    Task<bool> BulkAcknowledgeAlertsAsync(List<string> metricIds, string userId);
    Task<List<MonitoringDto>> GetAlertsByLevelAsync(AlertLevel alertLevel);
    
    // Threshold Management
    Task<bool> UpdateThresholdsAsync(string metricName, UpdateThresholdsRequest request);
    Task<ThresholdSettingsDto> GetThresholdSettingsAsync(string metricName);
    Task<List<string>> GetMetricsExceedingThresholdsAsync();
    
    // Performance Analytics
    Task<MetricStatisticsDto> GetMetricStatisticsAsync(string metricName, DateTime fromDate, DateTime toDate);
    Task<List<TrendAnalysisDto>> GetTrendAnalysisAsync(string metricName, DateTime fromDate, DateTime toDate);
    Task<List<PerformanceReportDto>> GetPerformanceReportAsync(string operationId, DateTime fromDate, DateTime toDate);
    
    // Business Metrics
    Task<BusinessMetricsDto> GetBusinessMetricsAsync(DateTime fromDate, DateTime toDate);
    Task<TransactionMetricsDto> GetTransactionMetricsAsync(DateTime fromDate, DateTime toDate);
    Task<ErrorMetricsDto> GetErrorMetricsAsync(DateTime fromDate, DateTime toDate);
    
    // Custom Metrics
    Task<bool> RegisterCustomMetricAsync(RegisterCustomMetricRequest request);
    Task<List<CustomMetricDefinitionDto>> GetCustomMetricDefinitionsAsync();
    Task<bool> UpdateCustomMetricDefinitionAsync(string metricName, UpdateCustomMetricDefinitionRequest request);
    Task<bool> DeleteCustomMetricDefinitionAsync(string metricName);
}