namespace UserService.Core.DTOs.RolePermission
{
    public class RolePermissionDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty; // Role name for convenience
        public string PermissionId { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty; // Permission name for convenience
    }
}