using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IExportRepository : IRepository<Export>
{
    Task<List<Export>> GetByReportIdAsync(string reportId);
    Task<List<Export>> GetByOrganizationAsync(string organizationId);
    Task<List<Export>> GetByStatusAsync(ExportStatus status);
    Task<List<Export>> GetByFormatAsync(ReportFormat format);
    Task<List<Export>> GetByTypeAsync(ExportType exportType);
    Task<List<Export>> GetByVisibilityAsync(ExportVisibility visibility);
    Task<List<Export>> GetByPriorityAsync(ExportPriority priority);
    Task<List<Export>> GetByGeneratedByAsync(string userId);
    Task<List<Export>> GetExpiredExportsAsync();
    Task<List<Export>> GetExportsForCleanupAsync(int retentionDays);
    Task<List<Export>> GetPendingExportsAsync();
    Task<List<Export>> GetFailedExportsAsync();
    Task<List<Export>> GetRecentExportsAsync(string organizationId, int days = 30);
    Task<List<Export>> GetExportsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<Export>> GetExportsByCategoryAsync(string category);
    Task<List<Export>> GetExportsByBusinessOwnerAsync(string businessOwner);
    Task<List<Export>> GetComplianceExportsAsync(string organizationId);
    Task<List<Export>> GetExportsRequiringAuditAsync();
    Task<List<Export>> GetExportsForDistributionAsync();
    Task<List<Export>> GetExportsForSyncAsync();
    Task<Export?> GetWithReportAsync(string exportId);
    Task<bool> UpdateDownloadStatsAsync(string exportId, string downloadedBy, string? ipAddress = null);
    Task<bool> MarkAsDistributedAsync(string exportId);
    Task<bool> MarkAsArchivedAsync(string exportId, string archivedBy);
    Task<long> GetTotalFileSizeAsync(string organizationId);
    Task<int> GetExportCountAsync(string organizationId);
    Task<List<Export>> GetLargestExportsAsync(string organizationId, int limit = 10);
}