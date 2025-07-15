using UserService.Core.Entities;

namespace UserService.Core.DTOs.Shift;

public class UpdateShiftDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public Entities.ShiftMode? Mode { get; set; }
}