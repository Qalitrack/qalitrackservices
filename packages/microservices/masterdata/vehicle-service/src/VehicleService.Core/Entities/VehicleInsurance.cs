namespace VehicleService.Core.Entities;

public enum InsuranceType
{
    Comprehensive,
    ThirdParty,
    ThirdPartyFireAndTheft,
    Commercial,
    Personal
}

public class VehicleInsurance : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public InsuranceType InsuranceType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal Deductible { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentContact { get; set; } = string.Empty;
    public string PolicyHolderName { get; set; } = string.Empty;
    public string? CoverageDetails { get; set; } // JSON or text describing coverage
    public string? Exclusions { get; set; } // JSON or text describing exclusions
    public bool IsActive { get; set; } = true;
    public bool IsExpired => EndDate < DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}