using System.ComponentModel.DataAnnotations;
using UserService.Core.Enums;

namespace UserService.Core.Entities;

public class ShiftInstance : BaseEntity
{
    [Required]
    public string ShiftId { get; set; } = string.Empty;
        
    [Required]
    public DateTime ScheduledDate { get; set; }
        
    [Required]
    public DateTime ScheduledStartTime { get; set; }
        
    [Required]
    public DateTime ScheduledEndTime { get; set; }
        
    public ShiftInstanceStatus Status { get; set; } = ShiftInstanceStatus.Scheduled;
    public string? Notes { get; set; }
        
    // Navigation properties
    public virtual Shift Shift { get; set; } = null!;
    public virtual ICollection<ShiftAttendance> Attendances { get; set; } = new List<ShiftAttendance>();
    public virtual ICollection<ShiftNotification> Notifications { get; set; } = new List<ShiftNotification>();
}
