namespace UserService.Core.DTOs.Roles
{
    public class UserRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty; // Role name for convenience
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow; // When the role was assigned

        // Optional: Add user details if needed (e.g., FullName)
        public string UserName { get; set; } = string.Empty; // Full name or other user details for convenience
    }
}