using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IReportService
{
    // Basic CRUD Operations
    Task<IEnumerable<ReportReadDto>> GetAllAsync();
    Task<ReportReadDto?> GetByIdAsync(string id);
    Task<ReportReadDto> CreateAsync(CreateReportDto dto);
    Task<ReportReadDto?> UpdateAsync(string id, UpdateReportDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // Report Management
    Task<List<ReportReadDto>> GetByOrganizationAsync(string organizationId);
    Task<List<ReportReadDto>> GetByTypeAsync(ReportType reportType);
    Task<List<ReportReadDto>> GetByStatusAsync(ReportStatus status);
    Task<List<ReportReadDto>> GetByBusinessOwnerAsync(string businessOwner);
    Task<List<ReportReadDto>> GetByPriorityAsync(ReportPriority priority);
    Task<List<ReportReadDto>> SearchReportsAsync(string searchTerm, string organizationId);
    Task<List<ReportReadDto>> GetRecentReportsAsync(string organizationId, int days = 30);
    
    // Report Generation
    Task<ReportGenerationResultDto> GenerateReportAsync(string reportId, GenerateReportRequest? request = null);
    Task<List<ReportGenerationResultDto>> GenerateBatchReportsAsync(List<string> reportIds);
    Task<ReportGenerationResultDto> GenerateReportFromTemplateAsync(string templateId, GenerateFromTemplateRequest request);
    Task<bool> RegenerateReportAsync(string reportId);
    Task<ReportPreviewDto> PreviewReportAsync(string reportId, PreviewReportRequest? request = null);
    
    // Scheduling Integration
    Task<List<ReportReadDto>> GetScheduledReportsAsync(string organizationId);
    Task<bool> ScheduleReportAsync(string reportId, ScheduleReportRequest request);
    Task<bool> UnscheduleReportAsync(string reportId);
    Task<bool> UpdateReportScheduleAsync(string reportId, UpdateScheduleRequest request);
    
    // Template Integration
    Task<List<ReportReadDto>> GetReportsByTemplateAsync(string templateId);
    Task<bool> ApplyTemplateToReportAsync(string reportId, string templateId);
    Task<bool> UpdateReportTemplateAsync(string reportId, UpdateTemplateRequest request);
    
    // Export Management
    Task<List<ExportReadDto>> GetReportExportsAsync(string reportId);
    Task<ExportReadDto> CreateExportAsync(string reportId, CreateExportRequest request);
    Task<bool> DeleteExportAsync(string exportId);
    
    // Data Integration
    Task<bool> ValidateDataSourcesAsync(string reportId);
    Task<DataSourceValidationDto> GetDataSourceStatusAsync(string reportId);
    Task<bool> RefreshReportDataAsync(string reportId);
    Task<ReportDataDto> GetReportDataAsync(string reportId, DataFilterRequest? filters = null);
    
    // Analytics and Performance
    Task<ReportUsageDto> GetReportUsageAsync(string reportId);
    Task<List<ReportUsageDto>> GetUsageAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<ReportPerformanceDto> GetReportPerformanceAsync(string reportId);
    Task<bool> OptimizeReportAsync(string reportId);
    
    // Business Intelligence
    Task<List<ReportInsightDto>> GetReportInsightsAsync(string reportId);
    Task<TrendAnalysisDto> GetReportTrendAnalysisAsync(string reportId, DateTime fromDate, DateTime toDate);
    Task<BenchmarkingDto> GetReportBenchmarkingAsync(string reportId, string? industry = null);
    
    // Collaboration and Sharing
    Task<ShareTokenDto> ShareReportAsync(string reportId, ShareReportRequest request);
    Task<bool> RevokeReportShareAsync(string shareToken);
    Task<ReportReadDto?> GetSharedReportAsync(string shareToken);
    Task<bool> GrantReportAccessAsync(string reportId, GrantAccessRequest request);
    Task<bool> RevokeReportAccessAsync(string reportId, string userId);
    Task<List<ReportAccessDto>> GetReportAccessListAsync(string reportId);
    
    // Compliance and Audit
    Task<List<ReportReadDto>> GetComplianceReportsAsync(string organizationId);
    Task<ComplianceStatusDto> GetReportComplianceStatusAsync(string reportId);
    Task<AuditTrailDto> GetReportAuditTrailAsync(string reportId);
    Task<bool> MarkReportForComplianceReviewAsync(string reportId, string reviewerId);
    
    // Quality Management
    Task<DataQualityDto> CheckReportDataQualityAsync(string reportId);
    Task<ValidationResultDto> ValidateReportAsync(string reportId);
    Task<bool> ApproveReportAsync(string reportId, string approverId, string? comments = null);
    Task<bool> RejectReportAsync(string reportId, string reviewerId, string reason);
    
    // Lifecycle Management
    Task<bool> ArchiveReportAsync(string reportId, string archivedBy);
    Task<bool> RestoreReportAsync(string reportId);
    Task<bool> PurgeReportAsync(string reportId);
    Task<List<ReportReadDto>> GetArchivedReportsAsync(string organizationId);
    
    // Configuration Management
    Task<bool> UpdateReportConfigurationAsync(string reportId, UpdateConfigurationRequest request);
    Task<ReportConfigurationDto> GetReportConfigurationAsync(string reportId);
    Task<bool> CloneReportAsync(string reportId, CloneReportRequest request);
    Task<bool> ImportReportAsync(ImportReportRequest request);
    Task<ReportExportConfigDto> ExportReportConfigAsync(string reportId);
}