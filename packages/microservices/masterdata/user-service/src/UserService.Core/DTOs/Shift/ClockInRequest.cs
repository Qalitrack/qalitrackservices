using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Shift;

public class ClockInRequest
{
    [Required]
    public string ShiftInstanceId { get; set; } = string.Empty;
        
    [Required]
    public string EmployeeId { get; set; } = string.Empty;
        
    public DateTime? ClockInTime { get; set; } = null; // Defaults to DateTime.Now if null
    public string? Notes { get; set; }
}