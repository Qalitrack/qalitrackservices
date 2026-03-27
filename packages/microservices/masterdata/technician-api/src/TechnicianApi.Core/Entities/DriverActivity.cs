using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class DriverActivity : BaseEntity
{
    [Required]
    public string DriverId { get; set; } = string.Empty;

    [Required]
    public DriverActivityType ActivityType { get; set; }

    public string? ActivityData { get; set; } // JSON string for additional data

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}

public enum DriverActivityType
{
    Login,
    TripCreate,
    TripStart,
    TripComplete,
    ExpenseAdd,
    ReceiptUpload,
    ProfileUpdate,
    MileageRecord,
    LocationUpdate,
    AppOpen,
    DocumentUpload
}
