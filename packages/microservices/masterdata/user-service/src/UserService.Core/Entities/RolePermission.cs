namespace UserService.Core.Entities;

public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public virtual Role Role { get; set; } = null!;

    public string PermissionId { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual Permission Permission { get; set; } = null!;
}