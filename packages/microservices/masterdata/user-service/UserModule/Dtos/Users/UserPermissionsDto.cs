namespace UserModule.Dtos.Users;

public class UserPermissionsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}