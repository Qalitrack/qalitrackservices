namespace UserService.Core.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;  // Name of the permission, e.g., "ManageUsers", "AssignShifts"
        public string? Description { get; set; }  // Optional description of what the permission allows

        // Seeded system permissions (e.g. "roles.manage") are referenced by name in
        // [Authorize(Policy = "...")] attributes across the backend — renaming/deleting
        // them would silently break authorization.
        public bool IsSystem { get; set; } = false;


        // Navigation property for RolePermissions
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();  // Many-to-many relationship with Role
        public virtual ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();  // Many-to-many relationship with User
    }
}