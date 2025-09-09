using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Driver.Entities;

/// <summary>
/// Simplified driver entity for cement weighbridge operations
/// </summary>
public class Driver : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? EmployeeId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}

// Enumerations
public enum DriverStatus
{
    Active,
    Inactive,
    Suspended,
    Terminated
}