namespace VehicleService.Core.Entities;

public enum DocumentType
{
    RegistrationCertificate,
    InsurancePolicy,
    InspectionCertificate,
    RoadworthyCertificate,
    PurchaseInvoice,
    ServiceRecord,
    TaxClearance,
    PermitLicense,
    Other
}

public class VehicleDocument : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // Path to the stored document file
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // PDF, JPG, PNG, etc.
    public long FileSize { get; set; } // in bytes
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
}