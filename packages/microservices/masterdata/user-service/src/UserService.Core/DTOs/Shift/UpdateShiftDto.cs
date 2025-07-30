using UserService.Core.Entities;

namespace UserService.Core.DTOs.Shift;

public class UpdateShiftDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int? DurationMinutes { get; set; } // If provided, EndTime will be recalculated
    
    public Entities.ShiftMode? Mode { get; set; }
    public bool? AutoRepeatDaily { get; set; }
}