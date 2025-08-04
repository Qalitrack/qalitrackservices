namespace UserService.Core.DTOs.Permissions
{
    public class UpdatePermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }  // Optional description
        
        
    }
}