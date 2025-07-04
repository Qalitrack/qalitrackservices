namespace ProductService.Core.DTOs;

public class ProductComplianceDto
{
    public string ProductId { get; set; } = string.Empty;
    public bool IsHazardous { get; set; }
    public string? HazmatClass { get; set; }
    public string? TransportRequirements { get; set; }
    public string? StorageRequirements { get; set; }
    public string? HandlingInstructions { get; set; }
    public string? EmergencyProcedures { get; set; }
    public List<ComplianceRequirementDto> Requirements { get; set; } = new List<ComplianceRequirementDto>();
}

public class ComplianceRequirementDto
{
    public string Id { get; set; } = string.Empty;
    public string ComplianceType { get; set; } = string.Empty;
    public string Regulation { get; set; } = string.Empty;
    public string? Authority { get; set; }
    public string? CertificationNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public string? Documents { get; set; }
    public string? Notes { get; set; }
}