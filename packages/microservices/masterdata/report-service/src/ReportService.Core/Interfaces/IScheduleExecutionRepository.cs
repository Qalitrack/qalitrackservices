using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IScheduleExecutionRepository : IRepository<ScheduleExecution>
{
    Task<List<ScheduleExecution>> GetByScheduleIdAsync(string scheduleId);
    Task<List<ScheduleExecution>> GetByOrganizationAsync(string organizationId);
    Task<List<ScheduleExecution>> GetByStatusAsync(ScheduleExecutionStatus status);
    Task<List<ScheduleExecution>> GetRunningExecutionsAsync();
    Task<List<ScheduleExecution>> GetFailedExecutionsAsync(string organizationId);
    Task<List<ScheduleExecution>> GetExecutionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<ScheduleExecution>> GetExecutionsByTriggeredByAsync(string triggeredBy);
    Task<List<ScheduleExecution>> GetExecutionsRequiringReviewAsync();
    Task<List<ScheduleExecution>> GetExecutionsWithAlertsAsync();
    Task<List<ScheduleExecution>> GetLongRunningExecutionsAsync(TimeSpan thresholdDuration);
    Task<List<ScheduleExecution>> GetRecentExecutionsAsync(string scheduleId, int limit = 10);
    Task<ScheduleExecution?> GetLatestExecutionAsync(string scheduleId);
    Task<ScheduleExecution?> GetLastSuccessfulExecutionAsync(string scheduleId);
    Task<ScheduleExecution?> GetWithScheduleAsync(string executionId);
    Task<bool> UpdateProgressAsync(string executionId, decimal progressPercentage, string? progressMessage = null);
    Task<bool> UpdateResourceUsageAsync(string executionId, long? memoryUsed = null, decimal? cpuUsage = null);
    Task<bool> MarkAsCompletedAsync(string executionId, ScheduleExecutionStatus status, string? resultSummary = null, string? errorMessage = null);
    Task<List<ScheduleExecution>> GetExecutionHistoryAsync(string scheduleId, int limit = 100);
    Task<Dictionary<ScheduleExecutionStatus, int>> GetExecutionStatsAsync(string scheduleId, DateTime? fromDate = null);
    Task<TimeSpan> GetAverageExecutionDurationAsync(string scheduleId, int lastNExecutions = 10);
    Task<decimal> GetSuccessRateAsync(string scheduleId, DateTime? fromDate = null);
    Task<List<ScheduleExecution>> GetExecutionsForCleanupAsync(int retentionDays);
}