using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IScheduleRepository : IRepository<Schedule>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Schedule?> GetByNameAsync(string name);
    Task<List<Schedule>> GetByOrganizationAsync(string organizationId);
    Task<List<Schedule>> GetByTypeAsync(ScheduleType scheduleType);
    Task<List<Schedule>> GetByStatusAsync(ScheduleStatus status);
    Task<List<Schedule>> GetActiveSchedulesAsync(string organizationId);
    Task<List<Schedule>> GetDueSchedulesAsync();
    Task<List<Schedule>> GetSchedulesByPriorityAsync(SchedulePriority priority);
    Task<List<Schedule>> GetSchedulesByOwnerAsync(string ownerId);
    Task<List<Schedule>> GetSchedulesByRecurrenceTypeAsync(RecurrenceType recurrenceType);
    Task<List<Schedule>> GetFailedSchedulesAsync(string organizationId);
    Task<List<Schedule>> GetSchedulesRequiringApprovalAsync();
    Task<List<Schedule>> GetSchedulesByNextRunDateAsync(DateTime fromDate, DateTime toDate);
    Task<List<Schedule>> GetSchedulesByCategoryAsync(string category);
    Task<Schedule?> GetWithExecutionsAsync(string scheduleId);
    Task<Schedule?> GetWithReportsAsync(string scheduleId);
    Task<bool> UpdateNextRunDateAsync(string scheduleId, DateTime nextRunDate);
    Task<bool> UpdateExecutionStatsAsync(string scheduleId, ScheduleExecutionStatus status, TimeSpan? duration = null);
    Task<List<Schedule>> GetSchedulesWithDependenciesAsync(string organizationId);
    Task<List<Schedule>> GetDependentSchedulesAsync(string scheduleId);
    Task<List<Schedule>> GetPrerequisiteSchedulesAsync(string scheduleId);
}