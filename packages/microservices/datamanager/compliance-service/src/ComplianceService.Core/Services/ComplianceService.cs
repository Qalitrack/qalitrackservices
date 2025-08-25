using AutoMapper;
using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;
using ComplianceService.Core.Interfaces;

namespace ComplianceService.Core.Services;

public class ComplianceService : IComplianceService
{
    private readonly IComplianceRepository _complianceRepository;
    private readonly IComplianceRulesEngine _rulesEngine;
    private readonly IViolationDetectionService _violationDetectionService;
    private readonly IMapper _mapper;

    public ComplianceService(
        IComplianceRepository complianceRepository,
        IComplianceRulesEngine rulesEngine,
        IViolationDetectionService violationDetectionService,
        IMapper mapper)
    {
        _complianceRepository = complianceRepository;
        _rulesEngine = rulesEngine;
        _violationDetectionService = violationDetectionService;
        _mapper = mapper;
    }

    public async Task<ComplianceDto> CreateComplianceCheckAsync(CreateComplianceRequest request)
    {
        var compliance = new Compliance
        {
            ComplianceName = request.ComplianceName,
            ComplianceType = request.ComplianceType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            OrganizationId = request.OrganizationId,
            Description = request.Description,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId,
            Priority = request.Priority,
            RegulatoryFramework = request.RegulatoryFramework,
            RegulatoryBody = request.RegulatoryBody,
            ComplianceStandardId = request.ComplianceStandardId,
            RequiresContinuousMonitoring = request.RequiresContinuousMonitoring,
            MonitoringFrequencyDays = request.MonitoringFrequencyDays,
            Metadata = request.Metadata,
            Status = ComplianceStatus.Pending
        };

        var createdCompliance = await _complianceRepository.AddAsync(compliance);
        return _mapper.Map<ComplianceDto>(createdCompliance);
    }

    public async Task<ComplianceDto?> GetComplianceAsync(string complianceId)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        return compliance == null ? null : _mapper.Map<ComplianceDto>(compliance);
    }

    public async Task<ComplianceDto> UpdateComplianceAsync(string complianceId, UpdateComplianceRequest request)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null)
            throw new ArgumentException("Compliance record not found", nameof(complianceId));

        if (!string.IsNullOrEmpty(request.Description))
            compliance.Description = request.Description;

        if (request.Status.HasValue)
        {
            compliance.Status = request.Status.Value;
            if (request.Status.Value == ComplianceStatus.Compliant || request.Status.Value == ComplianceStatus.Resolved)
                compliance.CompletedDate = DateTime.UtcNow;
        }

        if (request.CompletedDate.HasValue)
            compliance.CompletedDate = request.CompletedDate;

        if (request.DueDate.HasValue)
            compliance.DueDate = request.DueDate;

        if (!string.IsNullOrEmpty(request.AssignedUserId))
            compliance.AssignedUserId = request.AssignedUserId;

        if (request.Priority.HasValue)
            compliance.Priority = request.Priority.Value;

        if (!string.IsNullOrEmpty(request.NonComplianceReason))
            compliance.NonComplianceReason = request.NonComplianceReason;

        if (!string.IsNullOrEmpty(request.RecommendedActions))
            compliance.RecommendedActions = request.RecommendedActions;

        if (!string.IsNullOrEmpty(request.CompletedActions))
            compliance.CompletedActions = request.CompletedActions;

        if (request.RequiresContinuousMonitoring.HasValue)
            compliance.RequiresContinuousMonitoring = request.RequiresContinuousMonitoring.Value;

        if (request.NextCheckDate.HasValue)
            compliance.NextCheckDate = request.NextCheckDate;

        if (request.MonitoringFrequencyDays.HasValue)
            compliance.MonitoringFrequencyDays = request.MonitoringFrequencyDays;

        if (request.CheckResults != null)
            compliance.CheckResults = request.CheckResults;

        if (request.Metadata != null)
            compliance.Metadata = request.Metadata;

        if (request.Evidence != null)
            compliance.Evidence = request.Evidence;

        compliance.UpdatedAt = DateTime.UtcNow;

        var updatedCompliance = await _complianceRepository.UpdateAsync(compliance);
        return _mapper.Map<ComplianceDto>(updatedCompliance);
    }

    public async Task<bool> DeleteComplianceAsync(string complianceId)
    {
        return await _complianceRepository.DeleteByIdAsync(complianceId);
    }

    public async Task<List<ComplianceDto>> GetComplianceByEntityAsync(string entityType, string entityId)
    {
        var complianceRecords = await _complianceRepository.GetByEntityAsync(entityType, entityId);
        return _mapper.Map<List<ComplianceDto>>(complianceRecords);
    }

    public async Task<List<ComplianceDto>> GetComplianceByOrganizationAsync(string organizationId)
    {
        var complianceRecords = await _complianceRepository.GetByOrganizationAsync(organizationId);
        return _mapper.Map<List<ComplianceDto>>(complianceRecords);
    }

    public async Task<List<ComplianceDto>> GetComplianceByStatusAsync(ComplianceStatus status)
    {
        var complianceRecords = await _complianceRepository.GetByStatusAsync(status);
        return _mapper.Map<List<ComplianceDto>>(complianceRecords);
    }

    public async Task<List<ComplianceDto>> GetComplianceByTypeAsync(ComplianceType complianceType)
    {
        var complianceRecords = await _complianceRepository.GetByTypeAsync(complianceType);
        return _mapper.Map<List<ComplianceDto>>(complianceRecords);
    }

    public async Task<ComplianceResultDto> ExecuteComplianceCheckAsync(string entityType, string entityId, List<string>? ruleIds = null)
    {
        // Get applicable rules
        var rules = ruleIds != null 
            ? await _complianceRepository.GetRulesByIdsAsync(ruleIds)
            : await _complianceRepository.GetRulesByEntityTypeAsync(entityType);

        // Simple stub implementation for now
        var violations = new List<ComplianceViolation>();

        // Create compliance record
        var compliance = new Compliance
        {
            ComplianceName = $"Compliance Check - {entityType}:{entityId}",
            ComplianceType = DetermineComplianceType(entityType),
            EntityType = entityType,
            EntityId = entityId,
            Status = ComplianceStatus.Compliant, // Stub
            IsCompliant = true, // Stub: assume compliant
            CheckDate = DateTime.UtcNow,
            ComplianceScore = 100 // Stub: perfect score
        };

        if (violations.Any())
        {
            compliance.NonComplianceReason = string.Join("; ", violations.Select(v => v.Description));
        }

        await _complianceRepository.AddAsync(compliance);

        return new ComplianceResultDto
        {
            ComplianceId = compliance.Id,
            EntityType = entityType,
            EntityId = entityId,
            IsCompliant = compliance.IsCompliant,
            ComplianceScore = (double)compliance.ComplianceScore,
            CheckDate = compliance.CheckDate,
            RuleResults = new Dictionary<string, object>(), // Stub: empty results
            Violations = _mapper.Map<List<ComplianceViolationDto>>(violations)
        };
    }

    public async Task<List<ComplianceResultDto>> ExecuteBulkComplianceCheckAsync(List<ComplianceCheckRequest> requests)
    {
        var results = new List<ComplianceResultDto>();

        foreach (var request in requests)
        {
            var result = await ExecuteComplianceCheckAsync(request.EntityType, request.EntityId, request.RuleIds);
            results.Add(result);
        }

        return results;
    }

    public async Task<ComplianceResultDto> RevalidateComplianceAsync(string complianceId)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null)
            throw new ArgumentException("Compliance record not found", nameof(complianceId));

        return await ExecuteComplianceCheckAsync(compliance.EntityType, compliance.EntityId);
    }

    public async Task<ComplianceSummaryDto> GetComplianceSummaryAsync(string organizationId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var complianceRecords = await _complianceRepository.GetByOrganizationAndDateRangeAsync(
            organizationId, fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow);

        var summary = new ComplianceSummaryDto
        {
            OrganizationId = organizationId,
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow,
            TotalComplianceChecks = complianceRecords.Count,
            CompliantItems = complianceRecords.Count(c => c.Status == ComplianceStatus.Compliant),
            NonCompliantItems = complianceRecords.Count(c => c.Status == ComplianceStatus.NonCompliant),
            PartiallyCompliantItems = complianceRecords.Count(c => c.Status == ComplianceStatus.PartiallyCompliant),
            AverageRiskScore = complianceRecords.Any() ? complianceRecords.Average(c => c.RiskScore) : 0,
            HighRiskItems = complianceRecords.Count(c => c.RiskLevel >= RiskLevel.High),
            CriticalViolations = complianceRecords.Count(c => c.Priority == CompliancePriority.Critical)
        };

        summary.OverallComplianceRate = summary.TotalComplianceChecks > 0 
            ? (decimal)summary.CompliantItems / summary.TotalComplianceChecks * 100 
            : 0;

        return summary;
    }

    public async Task<bool> ApproveComplianceAsync(string complianceId, string userId)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.Status = ComplianceStatus.Compliant;
        compliance.CompletedDate = DateTime.UtcNow;
        compliance.UpdatedBy = userId;
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<bool> RejectComplianceAsync(string complianceId, string userId, string reason)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.Status = ComplianceStatus.NonCompliant;
        compliance.NonComplianceReason = reason;
        compliance.UpdatedBy = userId;
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<bool> EscalateComplianceAsync(string complianceId, string userId, string reason)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.Status = ComplianceStatus.Escalated;
        compliance.Priority = CompliancePriority.Critical;
        compliance.Notes = $"{compliance.Notes}\n[ESCALATED] {reason}";
        compliance.UpdatedBy = userId;
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<bool> ResolveComplianceAsync(string complianceId, string userId, string resolution)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.Status = ComplianceStatus.Resolved;
        compliance.CompletedDate = DateTime.UtcNow;
        compliance.CompletedActions = resolution;
        compliance.UpdatedBy = userId;
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<RiskAssessmentDto> PerformRiskAssessmentAsync(string entityType, string entityId)
    {
        // This would implement sophisticated risk assessment logic
        // For now, return a basic implementation
        var complianceHistory = await _complianceRepository.GetByEntityAsync(entityType, entityId);
        
        var riskScore = CalculateRiskScore(complianceHistory);
        var riskLevel = DetermineRiskLevel(riskScore);

        return new RiskAssessmentDto
        {
            EntityType = entityType,
            EntityId = entityId,
            RiskLevel = riskLevel,
            RiskScore = riskScore,
            RiskAssessment = $"Risk assessment based on {complianceHistory.Count} compliance records",
            AssessmentDate = DateTime.UtcNow,
            RiskFactors = GenerateRiskFactors(complianceHistory),
            RecommendedMitigations = GenerateRecommendations(riskLevel)
        };
    }

    public async Task<List<RiskAssessmentDto>> GetHighRiskItemsAsync(string organizationId)
    {
        var complianceRecords = await _complianceRepository.GetHighRiskItemsAsync(organizationId);
        return complianceRecords.Select(c => new RiskAssessmentDto
        {
            EntityType = c.EntityType,
            EntityId = c.EntityId,
            RiskLevel = c.RiskLevel,
            RiskScore = c.RiskScore,
            RiskAssessment = c.RiskAssessment ?? string.Empty,
            AssessmentDate = c.UpdatedAt
        }).ToList();
    }

    public async Task<ComplianceRiskReportDto> GetRiskReportAsync(string organizationId, DateTime fromDate, DateTime toDate)
    {
        var complianceRecords = await _complianceRepository.GetByOrganizationAndDateRangeAsync(organizationId, fromDate, toDate);
        
        return new ComplianceRiskReportDto
        {
            OrganizationId = organizationId,
            FromDate = fromDate,
            ToDate = toDate,
            OverallRiskScore = complianceRecords.Any() ? complianceRecords.Average(c => c.RiskScore) : 0,
            HighRiskItems = await GetHighRiskItemsAsync(organizationId)
        };
    }

    public async Task<bool> EnableContinuousMonitoringAsync(string complianceId, int frequencyDays)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.RequiresContinuousMonitoring = true;
        compliance.MonitoringFrequencyDays = frequencyDays;
        compliance.NextCheckDate = DateTime.UtcNow.AddDays(frequencyDays);
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<bool> DisableContinuousMonitoringAsync(string complianceId)
    {
        var compliance = await _complianceRepository.GetByIdAsync(complianceId);
        if (compliance == null) return false;

        compliance.RequiresContinuousMonitoring = false;
        compliance.MonitoringFrequencyDays = null;
        compliance.NextCheckDate = null;
        compliance.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAsync(compliance);
        return true;
    }

    public async Task<List<ComplianceDto>> GetItemsRequiringMonitoringAsync()
    {
        var items = await _complianceRepository.GetItemsRequiringMonitoringAsync();
        return _mapper.Map<List<ComplianceDto>>(items);
    }

    public async Task<MonitoringScheduleDto> GetMonitoringScheduleAsync(string organizationId)
    {
        var items = await _complianceRepository.GetMonitoringScheduleAsync(organizationId);
        
        return new MonitoringScheduleDto
        {
            OrganizationId = organizationId,
            ScheduledItems = items.Where(i => i.NextCheckDate > DateTime.UtcNow)
                .Select(i => new ScheduledMonitoring
                {
                    ComplianceId = i.Id,
                    ComplianceName = i.ComplianceName,
                    NextCheckDate = i.NextCheckDate!.Value,
                    FrequencyDays = i.MonitoringFrequencyDays ?? 30,
                    Priority = i.Priority
                }).ToList(),
            OverdueItems = items.Where(i => i.NextCheckDate <= DateTime.UtcNow)
                .Select(i => new OverdueMonitoring
                {
                    ComplianceId = i.Id,
                    ComplianceName = i.ComplianceName,
                    DueDate = i.NextCheckDate!.Value,
                    DaysOverdue = (DateTime.UtcNow - i.NextCheckDate!.Value).Days,
                    Priority = i.Priority
                }).ToList()
        };
    }

    // Helper methods
    private ComplianceType DetermineComplianceType(string entityType)
    {
        return entityType.ToLower() switch
        {
            "transaction" => ComplianceType.WeightCompliance,
            "vehicle" => ComplianceType.LicenseCompliance,
            "route" => ComplianceType.RouteCompliance,
            "weighbridge" => ComplianceType.OperationalCompliance,
            _ => ComplianceType.CustomCompliance
        };
    }

    private decimal CalculateComplianceScore(List<RuleEvaluationResult> results)
    {
        if (!results.Any()) return 0;
        return results.Average(r => r.Score);
    }

    private decimal CalculateRiskScore(List<Compliance> complianceHistory)
    {
        if (!complianceHistory.Any()) return 0;
        
        var nonCompliantCount = complianceHistory.Count(c => !c.IsCompliant);
        var totalCount = complianceHistory.Count;
        
        return (decimal)nonCompliantCount / totalCount * 100;
    }

    private RiskLevel DetermineRiskLevel(decimal riskScore)
    {
        return riskScore switch
        {
            >= 80 => RiskLevel.Extreme,
            >= 60 => RiskLevel.Critical,
            >= 40 => RiskLevel.High,
            >= 20 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private List<RiskFactor> GenerateRiskFactors(List<Compliance> complianceHistory)
    {
        var factors = new List<RiskFactor>();
        
        if (complianceHistory.Any(c => !c.IsCompliant))
        {
            factors.Add(new RiskFactor
            {
                FactorName = "Historical Non-Compliance",
                Score = complianceHistory.Count(c => !c.IsCompliant) * 10,
                Description = "Previous compliance failures increase risk",
                Impact = RiskLevel.High
            });
        }

        return factors;
    }

    private List<string> GenerateRecommendations(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Extreme => new List<string> { "Immediate review required", "Escalate to management", "Implement emergency controls" },
            RiskLevel.Critical => new List<string> { "Priority review required", "Enhanced monitoring", "Risk mitigation plan" },
            RiskLevel.High => new List<string> { "Regular monitoring", "Review processes", "Training recommended" },
            RiskLevel.Medium => new List<string> { "Standard monitoring", "Periodic review" },
            _ => new List<string> { "Continue current monitoring" }
        };
    }
}

// Supporting classes for the service
public class RuleEvaluationResult
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public decimal Score { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class RuleResultDto
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public decimal Score { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}