using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Permissions;

public class PermissionUpdateDto
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Category { get; set; }
}