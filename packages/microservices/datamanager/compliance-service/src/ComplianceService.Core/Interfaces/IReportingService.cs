using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;

namespace ComplianceService.Core.Interfaces;

public interface IReportingService
{
    // Report Management
    Task<ReportingDto> CreateReportAsync(CreateReportRequest request);
    Task<ReportingDto?> GetReportAsync(string reportId);
    Task<ReportingDto> UpdateReportAsync(string reportId, UpdateReportRequest request);
    Task<bool> DeleteReportAsync(string reportId);
    Task<List<ReportingDto>> GetReportsByOrganizationAsync(string organizationId);
    Task<List<ReportingDto>> GetReportsByTypeAsync(ReportType reportType);
    Task<List<ReportingDto>> GetReportsByStatusAsync(ReportStatus status);
    Task<List<ReportingDto>> GetReportsByPeriodAsync(DateTime fromDate, DateTime toDate);
    
    // Report Generation
    Task<ReportingDto> GenerateComplianceReportAsync(GenerateComplianceReportRequest request);
    Task<ReportingDto> GenerateViolationReportAsync(GenerateViolationReportRequest request);
    Task<ReportingDto> GenerateRiskAssessmentReportAsync(GenerateRiskReportRequest request);
    Task<ReportingDto> GenerateRegulatorySubmissionAsync(GenerateRegulatorySubmissionRequest request);
    Task<ReportingDto> GenerateExecutiveDashboardAsync(GenerateExecutiveDashboardRequest request);
    
    // Report Processing
    Task<bool> GenerateReportFileAsync(string reportId, string format = "PDF");
    Task<byte[]> GetReportFileAsync(string reportId);
    Task<bool> ValidateReportDataAsync(string reportId);
    Task<ReportValidationDto> GetReportValidationResultsAsync(string reportId);
    
    // Report Submission
    Task<bool> SubmitReportAsync(string reportId, string userId);
    Task<bool> SubmitToExternalSystemAsync(string reportId, string submissionMethod);
    Task<SubmissionStatusDto> GetSubmissionStatusAsync(string reportId);
    Task<bool> RetrySubmissionAsync(string reportId);
    
    // Report Analytics
    Task<ReportAnalyticsDto> GetReportAnalyticsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<List<TrendAnalysisDto>> GetComplianceTrendsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<List<MetricAnalysisDto>> GetKeyMetricsAsync(string organizationId, DateTime fromDate, DateTime toDate);
    Task<BenchmarkingDto> GetComplianceBenchmarkingAsync(string organizationId, string industry);
    
    // Scheduled Reporting
    Task<bool> ScheduleReportAsync(string reportId, ScheduleReportRequest request);
    Task<bool> UnscheduleReportAsync(string reportId);
    Task<List<ScheduledReportDto>> GetScheduledReportsAsync(string organizationId);
    Task<List<ReportingDto>> GetOverdueReportsAsync(string organizationId);
    
    // Report Templates
    Task<ReportTemplateDto> CreateReportTemplateAsync(CreateReportTemplateRequest request);
    Task<List<ReportTemplateDto>> GetReportTemplatesAsync();
    Task<ReportingDto> GenerateFromTemplateAsync(string templateId, Dictionary<string, object> parameters);
    
    // Regulatory Submissions
    Task<List<RegulatorySubmissionDto>> GetPendingSubmissionsAsync(string organizationId);
    Task<bool> AutoSubmitScheduledReportsAsync();
    Task<SubmissionHistoryDto> GetSubmissionHistoryAsync(string organizationId, DateTime fromDate, DateTime toDate);
}