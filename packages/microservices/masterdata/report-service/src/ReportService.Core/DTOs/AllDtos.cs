namespace ReportService.Core.DTOs;

// Export DTOs
public class ExportReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ExportGenerationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ExportErrorAnalysisDto
{
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime ErrorDate { get; set; }
}

public class ExportDashboardDto
{
    public string Title { get; set; } = string.Empty;
    public int TotalExports { get; set; }
}

public class ExportSummaryDto
{
    public string Summary { get; set; } = string.Empty;
    public int Count { get; set; }
}

// Report DTOs
public class ReportGenerationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReportAnalyticsDto
{
    public string ReportId { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}

public class ReportPerformanceDto
{
    public string ReportId { get; set; } = string.Empty;
    public double AverageGenerationTime { get; set; }
}

public class ReportDashboardDto
{
    public string Title { get; set; } = string.Empty;
    public int TotalReports { get; set; }
}

public class ReportSummaryDto
{
    public string Summary { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportTrendDto
{
    public string Period { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportInsightDto
{
    public string Insight { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class ReportGenerationStatsDto
{
    public int TotalGenerations { get; set; }
    public int SuccessfulGenerations { get; set; }
}

// Schedule DTOs
public class ScheduleReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ScheduleExecutionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ScheduleExecutionReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
}

public class ScheduleAnalyticsDto
{
    public string ScheduleId { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
}

public class SchedulePerformanceDto
{
    public string ScheduleId { get; set; } = string.Empty;
    public double AverageExecutionTime { get; set; }
}

public class ScheduleDashboardDto
{
    public string Title { get; set; } = string.Empty;
    public int TotalSchedules { get; set; }
}

public class ScheduleSummaryDto
{
    public string Summary { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ScheduleAvailabilityDto
{
    public bool IsAvailable { get; set; }
    public string Reason { get; set; } = string.Empty;
}

// Template DTOs
public class TemplateReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TemplateValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TemplateUsageStatsDto
{
    public string TemplateId { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class TemplatePerformanceDto
{
    public string TemplateId { get; set; } = string.Empty;
    public double AverageProcessingTime { get; set; }
}

public class TemplateGenerationStatsDto
{
    public int TotalGenerations { get; set; }
    public int SuccessfulGenerations { get; set; }
}

public class TemplateVersionDto
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class TemplateComparisonDto
{
    public string TemplateId1 { get; set; } = string.Empty;
    public string TemplateId2 { get; set; } = string.Empty;
    public string Differences { get; set; } = string.Empty;
}

public class TemplateAccessDto
{
    public string TemplateId { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
}

// Request DTOs
public class GenerateReportRequest
{
    public string ReportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class GenerateFromTemplateRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CreateVersionRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class ShareTemplateRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class BulkExportRequest
{
    public List<string> ReportIds { get; set; } = new();
    public string Format { get; set; } = string.Empty;
}

public class ScheduleExecutionRequest
{
    public string ScheduleId { get; set; } = string.Empty;
    public DateTime? ExecuteAt { get; set; }
}

public class BulkScheduleRequest
{
    public List<string> ScheduleIds { get; set; } = new();
    public DateTime ExecuteAt { get; set; }
}

// Analysis DTOs
public class TrendAnalysisDto
{
    public string Period { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class BenchmarkingDto
{
    public string Metric { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double BenchmarkValue { get; set; }
}

// Additional Request DTOs
public class GenerateExportRequest
{
    public string ExportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class BatchExportRequest
{
    public List<string> ExportIds { get; set; } = new();
    public string Format { get; set; } = string.Empty;
}

public class PreviewExportRequest
{
    public string ExportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class PdfExportRequest
{
    public string ExportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Settings { get; set; }
}

public class ExcelExportRequest
{
    public string ExportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Settings { get; set; }
}

public class CsvExportRequest
{
    public string ExportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Settings { get; set; }
}

public class PreviewReportRequest
{
    public string ReportId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ScheduleReportRequest
{
    public string ReportId { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
}

public class UpdateScheduleRequest
{
    public string ScheduleId { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
}

public class TemplateGenerationRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ValidateTemplateRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object>? ValidationRules { get; set; }
}

public class ImportScheduleRequest
{
    public string ScheduleData { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class CloneScheduleRequest
{
    public string SourceScheduleId { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
}

// Additional Response DTOs
public class ExportPreviewDto
{
    public string PreviewData { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class ReportPreviewDto
{
    public string PreviewData { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class ScheduleQueueDto
{
    public string ScheduleId { get; set; } = string.Empty;
    public int Position { get; set; }
}

public class QueueStatsDto
{
    public int TotalQueued { get; set; }
    public int Processing { get; set; }
}

public class ScheduleComplianceDto
{
    public string ScheduleId { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
}

public class BulkOperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
}

public class ScheduleExportDto
{
    public string ScheduleData { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

public class ScheduleOptimizationDto
{
    public string Recommendations { get; set; } = string.Empty;
    public double PerformanceGain { get; set; }
}

// More missing DTOs
public class BulkDistributionRequest
{
    public List<string> Recipients { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class ExternalSyncRequest
{
    public string SystemId { get; set; } = string.Empty;
    public Dictionary<string, object>? Settings { get; set; }
}

public class ExternalSyncStatusDto
{
    public bool IsSynced { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ExternalIntegrationConfig
{
    public string SystemType { get; set; } = string.Empty;
    public Dictionary<string, object>? Configuration { get; set; }
}

public class ExternalSystemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ExportAuditTrailDto
{
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class ComplianceReportDto
{
    public string ReportId { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string ComplianceNotes { get; set; } = string.Empty;
}

public class ExportTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
}

public class CreateExportTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
}

public class GenerateFromExportTemplateRequest
{
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

// Final batch of missing DTOs - all as minimal implementations
public class ImageExportRequest { public string Id { get; set; } = string.Empty; }
public class ImportTemplateRequest { public string Data { get; set; } = string.Empty; }
public class NotificationLogDto { public string Message { get; set; } = string.Empty; }
public class UpdateResourceLimitsRequest { public int MaxConcurrent { get; set; } }
public class ResourceUsageDto { public double CpuUsage { get; set; } }
public class ExportTemplateRequest { public string TemplateId { get; set; } = string.Empty; }
public class TemplateExportDto { public string Data { get; set; } = string.Empty; }
public class CloneTemplateRequest { public string SourceId { get; set; } = string.Empty; }
public class TemplateConfigDto { public string Configuration { get; set; } = string.Empty; }
public class TemplateLibraryFilter { public string Filter { get; set; } = string.Empty; }
public class PublishTemplateRequest { public string TemplateId { get; set; } = string.Empty; }
public class TemplateImpactAnalysisDto { public string Impact { get; set; } = string.Empty; }

// Mass add all remaining missing DTOs as minimal stub classes
public class CustomFormatExportRequest { public string Format { get; set; } = string.Empty; }
public class FileDownloadDto { public string FileName { get; set; } = string.Empty; }
public class ExportFileInfoDto { public string Info { get; set; } = string.Empty; }
public class GrantExportAccessRequest { public string UserId { get; set; } = string.Empty; }
public class ExportAccessDto { public bool HasAccess { get; set; } }
public class UpdatePermissionsRequest { public string UserId { get; set; } = string.Empty; }
public class SetPasswordRequest { public string Password { get; set; } = string.Empty; }
public class DistributeExportRequest { public string Method { get; set; } = string.Empty; }
public class ShareExportRequest { public string UserId { get; set; } = string.Empty; }
public class ShareTokenDto { public string Token { get; set; } = string.Empty; }
public class DistributionLogDto { public string Event { get; set; } = string.Empty; }
public class ReportConfigurationDto { public string Config { get; set; } = string.Empty; }
public class DataQualityDto { public double Score { get; set; } }
public class UpdateCustomizationRequest { public string Customization { get; set; } = string.Empty; }
public class UpdateNotificationRequest { public string NotificationSettings { get; set; } = string.Empty; }
public class NotificationTestRequest { public string TestType { get; set; } = string.Empty; }
public class UpdateConfigurationRequest { public string Config { get; set; } = string.Empty; }
public class PreviewTemplateRequest { public string TemplateId { get; set; } = string.Empty; }
public class TemplatePreviewDto { public string Preview { get; set; } = string.Empty; }
public class CloneReportRequest { public string SourceId { get; set; } = string.Empty; }
public class ImportReportRequest { public string Data { get; set; } = string.Empty; }
public class ReportExportConfigDto { public string Config { get; set; } = string.Empty; }

// Additional DTOs still needed for IReportService
public class UpdateTemplateRequest { public string TemplateId { get; set; } = string.Empty; }
public class CreateExportRequest { public string Name { get; set; } = string.Empty; }
public class ValidationResultDto { public bool IsValid { get; set; } }
public class DataSourceValidationDto { public bool IsValid { get; set; } }
public class DataFilterRequest { public string Filter { get; set; } = string.Empty; }
public class ReportDataDto { public string Data { get; set; } = string.Empty; }
public class ReportUsageDto { public int UsageCount { get; set; } }
public class ShareReportRequest { public string UserId { get; set; } = string.Empty; }
public class GrantAccessRequest { public string UserId { get; set; } = string.Empty; }
public class ReportAccessDto { public bool HasAccess { get; set; } }
public class ComplianceStatusDto { public bool IsCompliant { get; set; } }
public class AuditTrailDto { public string Action { get; set; } = string.Empty; }