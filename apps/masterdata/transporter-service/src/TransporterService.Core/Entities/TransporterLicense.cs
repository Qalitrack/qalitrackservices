namespace TransporterService.Core.Entities;

public enum LicenseType
{
    OperatingLicense,
    TransportPermit,
    HazmatLicense,
    InternationalPermit,
    SpecialCargo,
    Environmental,
    Safety
}

public enum LicenseStatus
{
    Active,
    Expired,
    Suspended,
    Revoked,
    Pending,
    UnderReview
}

public class TransporterLicense : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    public string? Description { get; set; }
    public string? Restrictions { get; set; }
    public decimal? Fee { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}