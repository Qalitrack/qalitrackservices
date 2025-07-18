using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface ITemplateRepository : IRepository<Template>
{
    Task<bool> IsNameAvailableAsync(string name);
    Task<Template?> GetByNameAsync(string name);
    Task<List<Template>> GetByOrganizationAsync(string organizationId);
    Task<List<Template>> GetByTypeAsync(TemplateType templateType);
    Task<List<Template>> GetByStatusAsync(TemplateStatus status);
    Task<List<Template>> GetSystemTemplatesAsync();
    Task<List<Template>> GetDefaultTemplatesAsync();
    Task<List<Template>> GetByIndustryAsync(string industry);
    Task<List<Template>> GetByCategoryAsync(string category);
    Task<List<Template>> GetByUseCaseAsync(string useCase);
    Task<List<Template>> GetActiveTemplatesAsync(string organizationId);
    Task<List<Template>> GetTemplatesByOwnerAsync(string ownerId);
    Task<List<Template>> GetApprovedTemplatesAsync();
    Task<List<Template>> GetTemplatesForReportTypeAsync(ReportType reportType);
    Task<List<Template>> SearchByNameAsync(string searchTerm);
    Task<Template?> GetWithReportsAsync(string templateId);
    Task<Template?> GetWithBaseTemplateAsync(string templateId);
    Task<List<Template>> GetDerivedTemplatesAsync(string baseTemplateId);
    Task<List<Template>> GetMostUsedTemplatesAsync(string organizationId, int limit = 10);
    Task<List<Template>> GetRecentlyUsedTemplatesAsync(string organizationId, int days = 30);
    Task<bool> UpdateUsageStatsAsync(string templateId);
}