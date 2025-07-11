namespace UserService.Core.DTOs.Roles;


    public class UpdateUserRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        // Optional: Add any additional properties to manage role changes, if required.
    }

