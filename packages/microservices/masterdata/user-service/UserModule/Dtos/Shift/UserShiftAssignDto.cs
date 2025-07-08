using System;
using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Shift;

public class UserShiftAssignDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid ShiftId { get; set; }
    
    [Required]
    public DateTime AssignedDate { get; set; }
    
    public string? Notes { get; set; }
}
