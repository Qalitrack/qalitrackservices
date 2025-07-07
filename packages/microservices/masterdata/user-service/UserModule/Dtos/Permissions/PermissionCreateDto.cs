using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Permissions;
public class PermissionCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string Category { get; set; } = "General";
}