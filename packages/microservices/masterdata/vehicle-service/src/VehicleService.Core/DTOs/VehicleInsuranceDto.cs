using VehicleService.Core.Entities;

namespace VehicleService.Core.DTOs;

public class VehicleInsuranceDto
{
    public string Id { get; set; } = string.Empty;
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
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateVehicleInsuranceRequest
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
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVehicleInsuranceRequest
{
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
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}