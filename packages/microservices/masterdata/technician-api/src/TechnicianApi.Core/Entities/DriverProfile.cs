using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class DriverProfile : BaseEntity
{
    [Required]
    public string DriverId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? IdNumber { get; set; }

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    public DateTime? LicenseExpiryDate { get; set; }

    // Image fields
    public string? ProfilePhotoUrl { get; set; }
    public string? LicenseFrontImageUrl { get; set; }
    public string? LicenseBackImageUrl { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }

    // Versioning
    public DriverProfileStatus Status { get; set; } = DriverProfileStatus.Draft;
    public bool IsCurrent { get; set; } = false;
    public int VersionNumber { get; set; } = 1;

    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }

    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    // Navigation properties
    public virtual ICollection<LicenseClass> LicenseClasses { get; set; } = new List<LicenseClass>();
    public virtual ICollection<DriverProfileChange> Changes { get; set; } = new List<DriverProfileChange>();

    // Computed property
    public int? DaysUntilLicenseExpiry => LicenseExpiryDate.HasValue
        ? (int)(LicenseExpiryDate.Value - DateTime.UtcNow).TotalDays
        : null;
}

public enum DriverProfileStatus
{
    Draft,
    Pending,
    Approved,
    Rejected,
    ChangesRequested
}
