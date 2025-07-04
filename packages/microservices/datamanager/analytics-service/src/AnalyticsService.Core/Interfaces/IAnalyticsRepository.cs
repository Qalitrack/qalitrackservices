using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IAnalyticsRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

public interface IAnalyticsMetricRepository : IAnalyticsRepository<AnalyticsMetric>
{
    Task<IEnumerable<AnalyticsMetric>> GetByOrganizationAsync(string organizationId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<AnalyticsMetric>> GetByMetricTypeAsync(string organizationId, string metricType, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<AnalyticsMetric>> GetByWeighbridgeAsync(string weighbridgeId, DateTime? from = null, DateTime? to = null);
    Task<AnalyticsMetric?> GetLatestMetricAsync(string organizationId, string metricType, string? weighbridgeId = null);
    Task<IEnumerable<AnalyticsMetric>> GetTimeSeriesAsync(string organizationId, string metricType, DateTime from, DateTime to, string? weighbridgeId = null);
}

public interface IDashboardWidgetRepository : IAnalyticsRepository<DashboardWidget>
{
    Task<IEnumerable<DashboardWidget>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<DashboardWidget>> GetByDashboardAsync(string dashboardId);
    Task<IEnumerable<DashboardWidget>> GetActiveWidgetsAsync(string organizationId);
}

public interface IReportDefinitionRepository : IAnalyticsRepository<ReportDefinition>
{
    Task<IEnumerable<ReportDefinition>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<ReportDefinition>> GetScheduledReportsAsync();
    Task<IEnumerable<ReportDefinition>> GetByReportTypeAsync(string reportType);
    Task<IEnumerable<ReportDefinition>> GetDueReportsAsync(DateTime currentTime);
}

public interface IMetricsAggregationRepository : IAnalyticsRepository<MetricsAggregation>
{
    Task<IEnumerable<MetricsAggregation>> GetAggregationsAsync(string organizationId, string metricType, string timeGranularity, DateTime from, DateTime to);
    Task<MetricsAggregation?> GetAggregationAsync(string organizationId, string metricType, string timeGranularity, DateTime periodStart, DateTime periodEnd);
    Task<IEnumerable<MetricsAggregation>> GetIncompleteAggregationsAsync();
}

public interface IPerformanceIndicatorRepository : IAnalyticsRepository<PerformanceIndicator>
{
    Task<IEnumerable<PerformanceIndicator>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<PerformanceIndicator>> GetByCategoryAsync(string organizationId, string category);
    Task<IEnumerable<PerformanceIndicator>> GetActiveIndicatorsAsync(string organizationId);
}

public interface ITrendAnalysisRepository : IAnalyticsRepository<TrendAnalysis>
{
    Task<IEnumerable<TrendAnalysis>> GetByMetricTypeAsync(string organizationId, string metricType);
    Task<TrendAnalysis?> GetLatestAnalysisAsync(string organizationId, string metricType, string? weighbridgeId = null);
    Task<IEnumerable<TrendAnalysis>> GetRecentAnalysesAsync(string organizationId, int days = 30);
}

public interface IBenchmarkDataRepository : IAnalyticsRepository<BenchmarkData>
{
    Task<IEnumerable<BenchmarkData>> GetByMetricTypeAsync(string metricType);
    Task<IEnumerable<BenchmarkData>> GetByBenchmarkTypeAsync(string benchmarkType);
    Task<BenchmarkData?> GetBenchmarkAsync(string metricType, string benchmarkType, string? industry = null, string? region = null);
    Task<IEnumerable<BenchmarkData>> GetActiveBenchmarksAsync();
}

public interface IAnomalyRepository : IAnalyticsRepository<Anomaly>
{
    Task<IEnumerable<Anomaly>> GetByOrganizationAsync(string organizationId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Anomaly>> GetByMetricTypeAsync(string organizationId, string metricType, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Anomaly>> GetBySeverityAsync(string organizationId, string severity);
    Task<IEnumerable<Anomaly>> GetOpenAnomaliesAsync(string organizationId);
    Task<IEnumerable<Anomaly>> GetUnnotifiedAnomaliesAsync();
    Task<int> GetAnomalyCountAsync(string organizationId, DateTime? from = null, DateTime? to = null);
}