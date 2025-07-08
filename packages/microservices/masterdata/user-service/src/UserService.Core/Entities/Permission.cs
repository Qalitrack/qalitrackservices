namespace UserService.Core.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionScope Scope { get; set; } = PermissionScope.System;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public enum PermissionScope
{
    System,
    Organization,
    User
}