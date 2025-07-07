using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Shift;

public class ShiftCreateDto
{   [Key]
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public TimeSpan StartTime { get; set; }
    [Required]
    public TimeSpan EndTime { get; set; }
    [Required]
    public bool IsActive { get; set; }
    [Required]
    public string Mode { get; set; } // "Strict" or "NonStrict"
}



