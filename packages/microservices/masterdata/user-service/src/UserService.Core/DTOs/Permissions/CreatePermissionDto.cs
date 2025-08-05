using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Permissions;

public class CreatePermissionDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    
    
}