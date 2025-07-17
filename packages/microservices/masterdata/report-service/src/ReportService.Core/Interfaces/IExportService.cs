using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IExportService
{
    // Basic CRUD Operations
    Task<IEnumerable<ExportReadDto>> GetAllAsync();
    Task<ExportReadDto?> GetByIdAsync(string id);
    Task<ExportReadDto> CreateAsync(CreateExportDto dto);
    Task<ExportReadDto?> UpdateAsync(string id, UpdateExportDto dto);
    Task<bool> DeleteAsync(string id);
    
    // Export Management
    Task<List<ExportReadDto>> GetByReportIdAsync(string reportId);
    Task<List<ExportReadDto>> GetByOrganizationAsync(string organizationId);
    Task<List<ExportReadDto>> GetByStatusAsync(ExportStatus status);
    Task<List<ExportReadDto>> GetByFormatAsync(ReportFormat format);
    Task<List<ExportReadDto>> GetByTypeAsync(ExportType exportType);
    Task<List<ExportReadDto>> GetRecentExportsAsync(string organizationId, int days = 30);
    
    // Export Generation
    Task<ExportGenerationResultDto> GenerateExportAsync(string reportId, GenerateExportRequest request);
    Task<List<ExportGenerationResultDto>> GenerateBatchExportsAsync(List<BatchExportRequest> requests);
    Task<ExportGenerationResultDto> RegenerateExportAsync(string exportId);
    Task<ExportPreviewDto> PreviewExportAsync(string reportId, PreviewExportRequest request);
    Task<bool> CancelExportGenerationAsync(string exportId);
    
    // Format-Specific Generation
    Task<ExportGenerationResultDto> GeneratePdfExportAsync(string reportId, PdfExportRequest request);
    Task<ExportGenerationResultDto> GenerateExcelExportAsync(string reportId, ExcelExportRequest request);
    Task<ExportGenerationResultDto> GenerateCsvExportAsync(string reportId, CsvExportRequest request);
    Task<ExportGenerationResultDto> GenerateImageExportAsync(string reportId, ImageExportRequest request);
    Task<ExportGenerationResultDto> GenerateCustomFormatExportAsync(string reportId, CustomFormatExportRequest request);
    
    // File Management
    Task<FileDownloadDto> DownloadExportAsync(string exportId, string? userId = null);
    Task<string> GetDownloadUrlAsync(string exportId, TimeSpan? expiresIn = null);
    Task<byte[]> GetExportContentAsync(string exportId);
    Task<ExportFileInfoDto> GetFileInfoAsync(string exportId);
    Task<bool> ValidateFileIntegrityAsync(string exportId);
    
    // Security and Access Control
    Task<bool> GrantExportAccessAsync(string exportId, GrantExportAccessRequest request);
    Task<bool> RevokeExportAccessAsync(string exportId, string userId);
    Task<List<ExportAccessDto>> GetExportAccessListAsync(string exportId);
    Task<bool> UpdateExportPermissionsAsync(string exportId, UpdatePermissionsRequest request);
    Task<bool> SetPasswordProtectionAsync(string exportId, SetPasswordRequest request);
    Task<bool> RemovePasswordProtectionAsync(string exportId);
    
    // Distribution and Sharing
    Task<bool> DistributeExportAsync(string exportId, DistributeExportRequest request);
    Task<ShareTokenDto> ShareExportAsync(string exportId, ShareExportRequest request);
    Task<bool> RevokeExportShareAsync(string shareToken);
    Task<ExportReadDto?> GetSharedExportAsync(string shareToken);
    Task<List<DistributionLogDto>> GetDistributionHistoryAsync(string exportId);
    
    // Lifecycle Management
    Task<List<ExportReadDto>> GetExpiredExportsAsync();
    Task<bool> ArchiveExportAsync(string exportId, string archivedBy);
    Task<bool> RestoreExportAsync(string exportId);
    Task<bool> PurgeExportAsync(string exportId);
    Task<List<ExportReadDto>> GetArchivedExportsAsync(string organizationId);
    Task<CleanupResultDto> CleanupExpiredExportsAsync(string organizationId);
    
    // Analytics and Monitoring
    Task<ExportUsageDto> GetExportUsageAsync(string exportId);
    Task<List<ExportUsageDto>> GetUsageAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<ExportPerformanceDto> GetExportPerformanceAsync(string exportId);
    Task<List<ExportPerformanceDto>> GetPerformanceAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<StorageUsageDto> GetStorageUsageAsync(string organizationId);
    
    // Quality and Validation
    Task<ExportQualityDto> ValidateExportQualityAsync(string exportId);
    Task<DataIntegrityDto> CheckDataIntegrityAsync(string exportId);
    Task<ExportComparisonDto> CompareExportsAsync(string exportId1, string exportId2);
    Task<bool> VerifyExportContentAsync(string exportId);
    
    // Optimization and Performance
    Task<bool> OptimizeExportAsync(string exportId);
    Task<CompressionResultDto> CompressExportAsync(string exportId, CompressionRequest request);
    Task<OptimizationRecommendationDto> GetOptimizationRecommendationsAsync(string exportId);
    Task<bool> ConvertExportFormatAsync(string exportId, ReportFormat targetFormat);
    
    // Batch Operations
    Task<BulkOperationResultDto> BulkDeleteExportsAsync(List<string> exportIds);
    Task<BulkOperationResultDto> BulkArchiveExportsAsync(List<string> exportIds, string archivedBy);
    Task<BulkOperationResultDto> BulkDistributeExportsAsync(List<string> exportIds, BulkDistributionRequest request);
    Task<BulkOperationResultDto> BulkUpdateStatusAsync(List<string> exportIds, ExportStatus status);
    
    // External Integration
    Task<bool> SyncToExternalSystemAsync(string exportId, ExternalSyncRequest request);
    Task<ExternalSyncStatusDto> GetExternalSyncStatusAsync(string exportId);
    Task<bool> ConfigureExternalIntegrationAsync(string exportId, ExternalIntegrationConfig config);
    Task<List<ExternalSystemDto>> GetAvailableExternalSystemsAsync();
    
    // Compliance and Audit
    Task<List<ExportReadDto>> GetComplianceExportsAsync(string organizationId);
    Task<ExportAuditTrailDto> GetExportAuditTrailAsync(string exportId);
    Task<ComplianceReportDto> GenerateComplianceReportAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<bool> MarkForComplianceReviewAsync(string exportId, string reviewerId);
    
    // Template Integration
    Task<List<ExportTemplateDto>> GetExportTemplatesAsync(ReportFormat? format = null);
    Task<ExportTemplateDto> CreateExportTemplateAsync(CreateExportTemplateRequest request);
    Task<ExportGenerationResultDto> GenerateFromTemplateAsync(string templateId, GenerateFromExportTemplateRequest request);
    Task<bool> ApplyTemplateToExportAsync(string exportId, string templateId);
    
    // Error Handling and Recovery
    Task<List<ExportReadDto>> GetFailedExportsAsync();
    Task<bool> RetryFailedExportAsync(string exportId);
    Task<ExportErrorAnalysisDto> AnalyzeExportErrorsAsync(string exportId);
    Task<bool> RecoverCorruptedExportAsync(string exportId);
    
    // Reporting and Dashboard
    Task<ExportDashboardDto> GetExportDashboardAsync(string organizationId);
    Task<List<ExportSummaryDto>> GetExportSummaryAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<TrendAnalysisDto> GetExportTrendAnalysisAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<BenchmarkingDto> GetExportBenchmarkingAsync(string organizationId, string? industry = null);
}