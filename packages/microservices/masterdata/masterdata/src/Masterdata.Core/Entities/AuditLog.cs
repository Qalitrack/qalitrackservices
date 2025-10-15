using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class AuditLog : BaseEntity
{
    [Required]
    public string EntityName { get; set; } = null!;
    
    [Required]
    public string EntityId { get; set; } = null!;
    
    [Required]
    public string Action { get; set; } = null!; // "Create", "Update", "Delete"
    
    [Column(TypeName = "jsonb")]
    public string? OldValues { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? NewValues { get; set; }
    
    public string? AffectedProperties { get; set; }
    
    public string? UserId { get; set; }
    
    public string? UserName { get; set; }
    
    public string? IpAddress { get; set; }
}
