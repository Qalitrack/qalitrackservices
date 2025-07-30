using UserService.Core.Entities;

namespace UserService.Core.DTOs.Shift;

public class CreateShiftDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public ShiftMode Mode { get; set; }
    public bool AutoRepeatDaily { get; set; } = false;
    public int DurationMinutes { get; set; } // Required - EndTime will be calculated from this
}