using UserModule.Dtos.Permissions;

namespace UserModule.Dtos.Roles;

public class RoleReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PermissionReadDto> Permissions { get; set; } = new();
}