using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace UserModule.Models;

[Table("user_shifts")]
[Index(nameof(UserId), IsUnique = true)]
public class UserShift
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid ShiftId { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime AssignedDate { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public Guid? AssignedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    
    [ForeignKey(nameof(ShiftId))]
    public Shift Shift { get; set; }
    
    [ForeignKey(nameof(CreatedBy))]
    public User? CreatedByUser { get; set; }
    
    [ForeignKey(nameof(UpdatedBy))]
    public User? UpdatedByUser { get; set; }
    
    [ForeignKey(nameof(AssignedBy))]
    public User? AssignedByUser { get; set; }
}