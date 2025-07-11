using UserService.Core.Entities;

namespace UserService.Core.DTOs.Shift;

public class ShiftDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ShiftMode Mode { get; set; }
    public bool IsActive { get; set; }
}