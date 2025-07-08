using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Roles;

public class RoleUpdateDto
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
}