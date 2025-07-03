using DriverService.Core.Entities;

namespace DriverService.Core.DTOs;

public class DriverLicenseDto
{
    public string Id { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string IssuingState { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; }
    public string Categories { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Endorsements { get; set; } = string.Empty;
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDue { get; set; }
    public int Points { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties
    public int DaysToExpiry => (ExpiryDate - DateTime.UtcNow).Days;
    public bool IsExpiringSoon => DaysToExpiry <= 30;
    public bool IsExpired => ExpiryDate < DateTime.UtcNow;
}

public class CreateDriverLicenseDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string IssuingState { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    public string Categories { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Endorsements { get; set; } = string.Empty;
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDue { get; set; }
    public int Points { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}

public class UpdateDriverLicenseDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string IssuingState { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; }
    public string Categories { get; set; } = string.Empty;
    public string Restrictions { get; set; } = string.Empty;
    public string Endorsements { get; set; } = string.Empty;
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDue { get; set; }
    public int Points { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysToExpiry { get; set; }
    public string Categories { get; set; } = string.Empty;
    public LicenseStatus Status { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
}