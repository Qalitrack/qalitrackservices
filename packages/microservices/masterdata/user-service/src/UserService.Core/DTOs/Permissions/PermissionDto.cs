namespace UserService.Core.DTOs.Permissions;

public class PermissionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? UpdatedBy { get; set; }
}