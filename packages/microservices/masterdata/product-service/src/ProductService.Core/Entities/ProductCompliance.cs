namespace ProductService.Core.Entities;

public class ProductCompliance : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string ComplianceType { get; set; } = string.Empty;
    public string Regulation { get; set; } = string.Empty;
    public string? Authority { get; set; }
    public string? CertificationNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ComplianceStatus Status { get; set; } = ComplianceStatus.Active;
    public string? Requirements { get; set; }
    public string? Documents { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}

public enum ComplianceStatus
{
    Active,
    Expired,
    Pending,
    Suspended
}