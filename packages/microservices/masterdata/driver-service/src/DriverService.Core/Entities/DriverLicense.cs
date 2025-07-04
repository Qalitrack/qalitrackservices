namespace DriverService.Core.Entities;

public enum LicenseStatus
{
    Active,
    Expired,
    Suspended,
    Revoked,
    Pending
}

public enum LicenseCategory
{
    A, // Motorcycles
    B, // Cars
    C, // Trucks
    D, // Buses
    E, // Trailers
    CDL_A, // Commercial - Heavy trucks with trailers
    CDL_B, // Commercial - Large trucks, buses
    CDL_C  // Commercial - Hazmat, passenger vans
}

public class DriverLicense : BaseEntity
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string IssuingState { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    public string Categories { get; set; } = string.Empty; // JSON array of LicenseCategory
    public string Restrictions { get; set; } = string.Empty;
    public string Endorsements { get; set; } = string.Empty;
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDue { get; set; }
    public int Points { get; set; } = 0; // Penalty points
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}