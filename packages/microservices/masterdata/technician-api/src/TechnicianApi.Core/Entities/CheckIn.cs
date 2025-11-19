namespace TechnicianApi.Core.Entities;

public class CheckIn : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutTime { get; set; }
    public string? Notes { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.OnSite;

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum CheckInStatus
{
    OnSite,
    CheckedOut
}
