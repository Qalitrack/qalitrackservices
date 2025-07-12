namespace UserService.Core.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;  // Name of the permission, e.g., "ManageUsers", "AssignShifts"
        public string? Description { get; set; }  // Optional description of what the permission allows
        
        // Navigation property for RolePermissions
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();  // Many-to-many relationship with Role
        public virtual ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();  // Many-to-many relationship with User
    }
}