using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;

namespace ComplianceService.Core.Interfaces;

public interface IComplianceService
{
    // Compliance Management
    Task<ComplianceDto> CreateComplianceCheckAsync(CreateComplianceRequest request);
    Task<ComplianceDto?> GetComplianceAsync(string complianceId);
    Task<ComplianceDto> UpdateComplianceAsync(string complianceId, UpdateComplianceRequest request);
    Task<bool> DeleteComplianceAsync(string complianceId);
    Task<List<ComplianceDto>> GetComplianceByEntityAsync(string entityType, string entityId);
    Task<List<ComplianceDto>> GetComplianceByOrganizationAsync(string organizationId);
    Task<List<ComplianceDto>> GetComplianceByStatusAsync(ComplianceStatus status);
    Task<List<ComplianceDto>> GetComplianceByTypeAsync(ComplianceType complianceType);
    
    // Compliance Execution
    Task<ComplianceResultDto> ExecuteComplianceCheckAsync(string entityType, string entityId, List<string>? ruleIds = null);
    Task<List<ComplianceResultDto>> ExecuteBulkComplianceCheckAsync(List<ComplianceCheckRequest> requests);
    Task<ComplianceResultDto> RevalidateComplianceAsync(string complianceId);
    Task<ComplianceSummaryDto> GetComplianceSummaryAsync(string organizationId, DateTime? fromDate = null, DateTime? toDate = null);
    
    // Compliance Status Management
    Task<bool> ApproveComplianceAsync(string complianceId, string userId);
    Task<bool> RejectComplianceAsync(string complianceId, string userId, string reason);
    Task<bool> EscalateComplianceAsync(string complianceId, string userId, string reason);
    Task<bool> ResolveComplianceAsync(string complianceId, string userId, string resolution);
    
    // Risk Assessment
    Task<RiskAssessmentDto> PerformRiskAssessmentAsync(string entityType, string entityId);
    Task<List<RiskAssessmentDto>> GetHighRiskItemsAsync(string organizationId);
    Task<ComplianceRiskReportDto> GetRiskReportAsync(string organizationId, DateTime fromDate, DateTime toDate);
    
    // Continuous Monitoring
    Task<bool> EnableContinuousMonitoringAsync(string complianceId, int frequencyDays);
    Task<bool> DisableContinuousMonitoringAsync(string complianceId);
    Task<List<ComplianceDto>> GetItemsRequiringMonitoringAsync();
    Task<MonitoringScheduleDto> GetMonitoringScheduleAsync(string organizationId);
}