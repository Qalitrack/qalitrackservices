using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;

namespace ComplianceService.Core.Interfaces;

public interface IRegulatoryService
{
    // Regulatory Management
    Task<RegulatoryDto> CreateRegulatoryAsync(CreateRegulatoryRequest request);
    Task<RegulatoryDto?> GetRegulatoryAsync(string regulatoryId);
    Task<RegulatoryDto> UpdateRegulatoryAsync(string regulatoryId, UpdateRegulatoryRequest request);
    Task<bool> DeleteRegulatoryAsync(string regulatoryId);
    Task<List<RegulatoryDto>> GetRegulatoryByTypeAsync(RegulatoryType regulatoryType);
    Task<List<RegulatoryDto>> GetRegulatoryByJurisdictionAsync(string jurisdiction);
    Task<List<RegulatoryDto>> GetActiveRegulatoryAsync();
    Task<List<RegulatoryDto>> GetExpiredRegulatoryAsync();
    
    // Regulatory Rules
    Task<List<ComplianceRuleDto>> GetRegulatoryRulesAsync(string regulatoryId);
    Task<ComplianceRuleDto> AddRuleToRegulatoryAsync(string regulatoryId, CreateComplianceRuleRequest request);
    Task<bool> RemoveRuleFromRegulatoryAsync(string regulatoryId, string ruleId);
    
    // External Integration
    Task<RegulatoryValidationDto> ValidateWithExternalSystemAsync(string regulatoryId, Dictionary<string, object> data);
    Task<List<RegulatoryUpdateDto>> SyncWithExternalSystemsAsync();
    Task<bool> UpdateFromExternalSourceAsync(string regulatoryId);
    
    // Regulatory Review and Versioning
    Task<RegulatoryDto> CreateNewVersionAsync(string regulatoryId, CreateRegulatoryVersionRequest request);
    Task<List<RegulatoryDto>> GetRegulatoryVersionsAsync(string regulatoryCode);
    Task<bool> SupersedeRegulatoryAsync(string oldRegulatoryId, string newRegulatoryId);
    Task<RegulatoryComparisonDto> CompareRegulatoryVersionsAsync(string regulatoryId1, string regulatoryId2);
    
    // Regulatory Reporting
    Task<RegulatoryComplianceReportDto> GenerateComplianceReportAsync(string regulatoryId, DateTime fromDate, DateTime toDate);
    Task<List<RegulatoryDto>> GetRegulatoryRequiringReviewAsync();
    Task<RegulatoryEffectivenessDto> AnalyzeRegulatoryEffectivenessAsync(string regulatoryId, DateTime fromDate, DateTime toDate);
}