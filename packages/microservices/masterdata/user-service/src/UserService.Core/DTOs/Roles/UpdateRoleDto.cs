namespace UserService.Core.DTOs.Roles
{
    public class UpdateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}