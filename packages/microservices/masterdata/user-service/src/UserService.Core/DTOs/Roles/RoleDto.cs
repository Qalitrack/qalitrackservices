namespace UserService.Core.DTOs.Roles
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }  // Optional description of the role

        // Optional: You could return permissions assigned to this role, if needed.
        // public IEnumerable<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}