using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserModule.Models;

public enum ShiftMode
{
    Strict,    // Enforces strict rules (e.g., login only during shift)
    NonStrict  // Allows flexible login
}

[Table("shifts")]
public class Shift
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public ShiftMode Mode { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
}