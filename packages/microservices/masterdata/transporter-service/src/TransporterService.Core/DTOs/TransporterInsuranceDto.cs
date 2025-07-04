using TransporterService.Core.Entities;

namespace TransporterService.Core.DTOs;

public class TransporterInsuranceDto
{
    public string Id { get; set; } = string.Empty;
    public string TransporterId { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public InsuranceType InsuranceType { get; set; }
    public string InsuranceCompany { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public InsuranceStatus Status { get; set; }
    public string? Deductible { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? AgentName { get; set; }
    public string? AgentContact { get; set; }
    public DateTime? LastClaimDate { get; set; }
    public decimal? TotalClaimsAmount { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}