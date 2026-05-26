namespace UserService.Core.Entities;

public class UserPermissions:BaseEntity
{
    public required string UserId { get; set; }
    public required string PermissionName { get; set; }

    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    
}