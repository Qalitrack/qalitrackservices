using AnalyticsService.Core.DTOs;
using AnalyticsService.Core.Entities;

namespace AnalyticsService.Core.Interfaces;

public interface IAnalyticsService
{
    // Analytics Management
    Task<AnalyticsDto> CreateAnalyticsAsync(CreateAnalyticsRequest request);
    Task<AnalyticsDto?> GetAnalyticsAsync(string analyticsId);
    Task<AnalyticsDto> UpdateAnalyticsAsync(string analyticsId, UpdateAnalyticsRequest request);
    Task<bool> DeleteAnalyticsAsync(string analyticsId);
    Task<List<AnalyticsDto>> GetAnalyticsByOrganizationAsync(string organizationId);
    Task<List<AnalyticsDto>> GetAnalyticsByTypeAsync(AnalyticsType analyticsType);
    Task<List<AnalyticsDto>> GetAnalyticsByStatusAsync(AnalyticsStatus status);
    
    // Data Aggregation
    Task<AnalyticsResultDto> ExecuteAnalyticsAsync(string analyticsId);
    Task<AnalyticsResultDto> ExecuteCustomAnalyticsAsync(CustomAnalyticsRequest request);
    Task<List<AnalyticsResultDto>> ExecuteBulkAnalyticsAsync(List<string> analyticsIds);
    Task<AnalyticsResultDto> RefreshAnalyticsAsync(string analyticsId);
    
    // Real-time Analytics
    Task<bool> EnableRealTimeAnalyticsAsync(string analyticsId);
    Task<bool> DisableRealTimeAnalyticsAsync(string analyticsId);
    Task<List<AnalyticsDto>> GetRealTimeAnalyticsAsync(string organizationId);
    Task<RealTimeDataDto> GetRealTimeDataAsync(string analyticsId);
    
    // Scheduled Analytics
    Task<bool> ScheduleAnalyticsAsync(string analyticsId, ScheduleRequest schedule);
    Task<bool> UnscheduleAnalyticsAsync(string analyticsId);
    Task<List<ScheduledAnalyticsDto>> GetScheduledAnalyticsAsync(string organizationId);
    Task<bool> ExecuteScheduledAnalyticsAsync();
    
    // Analytics Performance
    Task<AnalyticsPerformanceDto> GetAnalyticsPerformanceAsync(string analyticsId);
    Task<List<AnalyticsPerformanceDto>> GetPerformanceSummaryAsync(string organizationId);
    Task<bool> OptimizeAnalyticsAsync(string analyticsId);
    
    // Data Integration
    Task<List<DataSourceDto>> GetAvailableDataSourcesAsync();
    Task<bool> ValidateDataSourceAsync(string dataSource);
    Task<DataQualityDto> CheckDataQualityAsync(string analyticsId);
    Task<DataLineageDto> GetDataLineageAsync(string analyticsId);
}