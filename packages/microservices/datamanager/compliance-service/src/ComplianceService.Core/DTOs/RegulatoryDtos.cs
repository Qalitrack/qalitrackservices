namespace ComplianceService.Core.DTOs;

public class RegulatoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class ComplianceRuleDto
{
    public string Id { get; set; } = string.Empty;
    public string RegulatoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class CreateRegulatoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateRegulatoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateComplianceRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class RegulatoryValidationDto
{
    public bool IsValid { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
    public Dictionary<string, object> Results { get; set; } = new();
}

public class RegulatoryUpdateDto
{
    public string RegulatoryId { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty;
    public DateTime UpdateDate { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
}

public class CreateRegulatoryVersionRequest
{
    public string VersionNotes { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
}

public class RegulatoryComparisonDto
{
    public string Regulatory1Id { get; set; } = string.Empty;
    public string Regulatory2Id { get; set; } = string.Empty;
    public List<string> Differences { get; set; } = new();
    public Dictionary<string, object> ComparisonResults { get; set; } = new();
}

public class RegulatoryComplianceReportDto
{
    public string RegulatoryId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> ComplianceMetrics { get; set; } = new();
    public List<string> Violations { get; set; } = new();
}

public class RegulatoryEffectivenessDto
{
    public string RegulatoryId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public double EffectivenessScore { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}