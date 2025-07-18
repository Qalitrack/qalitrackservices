using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IScheduleService
{
    // Basic CRUD Operations
    Task<IEnumerable<ScheduleReadDto>> GetAllAsync();
    Task<ScheduleReadDto?> GetByIdAsync(string id);
    Task<ScheduleReadDto> CreateAsync(CreateScheduleDto dto);
    Task<ScheduleReadDto?> UpdateAsync(string id, UpdateScheduleDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // Schedule Management
    Task<List<ScheduleReadDto>> GetByOrganizationAsync(string organizationId);
    Task<List<ScheduleReadDto>> GetByTypeAsync(ScheduleType scheduleType);
    Task<List<ScheduleReadDto>> GetByStatusAsync(ScheduleStatus status);
    Task<List<ScheduleReadDto>> GetActiveSchedulesAsync(string organizationId);
    Task<List<ScheduleReadDto>> GetByPriorityAsync(SchedulePriority priority);
    Task<List<ScheduleReadDto>> SearchSchedulesAsync(string searchTerm, string organizationId);
    
    // Schedule Execution
    Task<List<ScheduleReadDto>> GetDueSchedulesAsync();
    Task<ScheduleExecutionResultDto> ExecuteScheduleAsync(string scheduleId, bool force = false);
    Task<List<ScheduleExecutionResultDto>> ExecuteBatchSchedulesAsync(List<string> scheduleIds);
    Task<bool> CancelScheduleExecutionAsync(string scheduleId);
    Task<ScheduleExecutionStatusDto> GetExecutionStatusAsync(string scheduleId);
    
    // Schedule Configuration
    Task<bool> EnableScheduleAsync(string scheduleId);
    Task<bool> DisableScheduleAsync(string scheduleId);
    Task<bool> PauseScheduleAsync(string scheduleId);
    Task<bool> ResumeScheduleAsync(string scheduleId);
    Task<bool> UpdateNextRunDateAsync(string scheduleId, DateTime nextRunDate);
    Task<ScheduleConfigurationDto> GetScheduleConfigurationAsync(string scheduleId);
    
    // Recurrence and Timing
    Task<bool> UpdateRecurrenceAsync(string scheduleId, UpdateRecurrenceRequest request);
    Task<List<DateTime>> GetNextExecutionDatesAsync(string scheduleId, int count = 10);
    Task<bool> ValidateRecurrencePatternAsync(RecurrencePatternRequest pattern);
    Task<ScheduleCalendarDto> GetScheduleCalendarAsync(string organizationId, DateTime fromDate, DateTime toDate);
    
    // Dependencies Management
    Task<bool> AddDependencyAsync(string scheduleId, string dependentScheduleId);
    Task<bool> RemoveDependencyAsync(string scheduleId, string dependentScheduleId);
    Task<List<ScheduleReadDto>> GetDependentSchedulesAsync(string scheduleId);
    Task<List<ScheduleReadDto>> GetPrerequisiteSchedulesAsync(string scheduleId);
    Task<ScheduleDependencyMapDto> GetDependencyMapAsync(string organizationId);
    Task<bool> ValidateDependenciesAsync(string scheduleId);
    
    // Monitoring and Analytics
    Task<SchedulePerformanceDto> GetSchedulePerformanceAsync(string scheduleId);
    Task<List<SchedulePerformanceDto>> GetPerformanceAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<ScheduleHealthDto> GetScheduleHealthAsync(string scheduleId);
    Task<List<ScheduleAlertDto>> GetScheduleAlertsAsync(string organizationId);
    
    // Execution History
    Task<List<ScheduleExecutionDto>> GetExecutionHistoryAsync(string scheduleId, int limit = 100);
    Task<ScheduleExecutionDto?> GetLastExecutionAsync(string scheduleId);
    Task<ScheduleExecutionDto?> GetLastSuccessfulExecutionAsync(string scheduleId);
    Task<ScheduleExecutionStatsDto> GetExecutionStatsAsync(string scheduleId, DateTime? fromDate = null);
    Task<decimal> GetSuccessRateAsync(string scheduleId, DateTime? fromDate = null);
    
    // Failure Management
    Task<List<ScheduleReadDto>> GetFailedSchedulesAsync(string organizationId);
    Task<bool> RetryFailedScheduleAsync(string scheduleId);
    Task<bool> ResetScheduleAsync(string scheduleId);
    Task<FailureAnalysisDto> AnalyzeFailuresAsync(string scheduleId, DateTime? fromDate = null);
    
    // Notification Management
    Task<bool> UpdateNotificationSettingsAsync(string scheduleId, UpdateNotificationRequest request);
    Task<bool> TestNotificationAsync(string scheduleId, NotificationTestRequest request);
    Task<List<NotificationLogDto>> GetNotificationHistoryAsync(string scheduleId);
    
    // Resource Management
    Task<bool> UpdateResourceLimitsAsync(string scheduleId, UpdateResourceLimitsRequest request);
    Task<ResourceUsageDto> GetResourceUsageAsync(string scheduleId);
    Task<List<ResourceUsageDto>> GetResourceUsageAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    
    // Queue Management
    Task<List<ScheduleQueueDto>> GetScheduleQueueAsync(string? queueName = null);
    Task<bool> MoveToQueueAsync(string scheduleId, string queueName);
    Task<QueueStatsDto> GetQueueStatsAsync(string queueName);
    Task<bool> PrioritizeScheduleAsync(string scheduleId);
    
    // Approval and Compliance
    Task<List<ScheduleReadDto>> GetSchedulesRequiringApprovalAsync();
    Task<bool> ApproveScheduleAsync(string scheduleId, string approvedBy, string? comments = null);
    Task<bool> RejectScheduleAsync(string scheduleId, string reviewerId, string reason);
    Task<ScheduleComplianceDto> GetComplianceStatusAsync(string scheduleId);
    
    // Bulk Operations
    Task<BulkOperationResultDto> BulkEnableSchedulesAsync(List<string> scheduleIds);
    Task<BulkOperationResultDto> BulkDisableSchedulesAsync(List<string> scheduleIds);
    Task<BulkOperationResultDto> BulkDeleteSchedulesAsync(List<string> scheduleIds);
    Task<BulkOperationResultDto> BulkUpdateStatusAsync(List<string> scheduleIds, ScheduleStatus status);
    
    // Import and Export
    Task<ScheduleReadDto> ImportScheduleAsync(ImportScheduleRequest request);
    Task<ScheduleExportDto> ExportScheduleAsync(string scheduleId);
    Task<bool> CloneScheduleAsync(string scheduleId, CloneScheduleRequest request);
    
    // Optimization and Maintenance
    Task<bool> OptimizeScheduleAsync(string scheduleId);
    Task<ScheduleOptimizationDto> GetOptimizationRecommendationsAsync(string scheduleId);
    Task<bool> ArchiveScheduleAsync(string scheduleId, string archivedBy);
    Task<bool> RestoreScheduleAsync(string scheduleId);
    Task<List<ScheduleReadDto>> GetArchivedSchedulesAsync(string organizationId);
    
    // Dashboard and Reporting
    Task<ScheduleDashboardDto> GetScheduleDashboardAsync(string organizationId);
    Task<List<ScheduleSummaryDto>> GetScheduleSummaryAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<ScheduleAvailabilityDto> GetScheduleAvailabilityAsync(string organizationId, DateTime fromDate, DateTime toDate);
}