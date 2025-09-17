using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class ShiftInstanceResponse
{
    public string Id { get; set; } = string.Empty;
    public string ShiftId { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public ShiftInstanceStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
