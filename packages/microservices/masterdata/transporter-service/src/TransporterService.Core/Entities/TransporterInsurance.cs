namespace TransporterService.Core.Entities;

public enum InsuranceType
{
    Liability,
    Cargo,
    Comprehensive,
    ThirdParty,
    PersonalAccident,
    Goods,
    Professional
}

public enum InsuranceStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended,
    Pending
}

public class TransporterInsurance : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public InsuranceType InsuranceType { get; set; }
    public string InsuranceCompany { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public InsuranceStatus Status { get; set; } = InsuranceStatus.Active;
    public string? Deductible { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? AgentName { get; set; }
    public string? AgentContact { get; set; }
    public DateTime? LastClaimDate { get; set; }
    public decimal? TotalClaimsAmount { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}