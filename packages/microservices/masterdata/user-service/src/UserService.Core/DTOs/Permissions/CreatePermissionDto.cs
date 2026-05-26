using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Permissions;

public class CreatePermissionDto
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Description { get; set; }
    
    
}