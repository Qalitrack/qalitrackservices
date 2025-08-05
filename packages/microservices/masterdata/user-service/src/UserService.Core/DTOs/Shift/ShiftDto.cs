// In ShiftDto.cs

using UserService.Core.Entities;

namespace UserService.Core.DTOs.Shift;

public class ShiftDto
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }  // Using DateTime
    public DateTime EndTime { get; set; }    // Using DateTime
    public ShiftMode Mode { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRepeatDaily { get; set; }
    public int? DurationMinutes { get; set; }
    public string?CreatedAt { get; set; }
    public string?CreatedBy { get; set; }
    public string?UpdatedAt { get; set; }
    public string?UpdatedBy { get; set; }
    public string?IsDeleted { get; set; }
}