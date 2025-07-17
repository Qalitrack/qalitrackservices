using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Report?> GetByNameAsync(string name);
    Task<List<Report>> GetByOrganizationAsync(string organizationId);
    Task<List<Report>> GetByTypeAsync(ReportType reportType);
    Task<List<Report>> GetByStatusAsync(ReportStatus status);
    Task<List<Report>> GetByTemplateIdAsync(string templateId);
    Task<List<Report>> GetScheduledReportsAsync(string organizationId);
    Task<List<Report>> GetByBusinessOwnerAsync(string businessOwner);
    Task<List<Report>> GetByPriorityAsync(ReportPriority priority);
    Task<List<Report>> GetRecentlyGeneratedAsync(string organizationId, int days = 30);
    Task<List<Report>> GetByVisibilityAsync(ReportVisibility visibility);
    Task<List<Report>> GetByFormatAsync(ReportFormat format);
    Task<List<Report>> SearchByNameAsync(string searchTerm);
    Task<List<Report>> GetByTagsAsync(List<string> tags);
    Task<List<Report>> GetDueForGenerationAsync();
    Task<Report?> GetWithExportsAsync(string reportId);
    Task<Report?> GetWithTemplateAsync(string reportId);
    Task<Report?> GetWithScheduleAsync(string reportId);
}