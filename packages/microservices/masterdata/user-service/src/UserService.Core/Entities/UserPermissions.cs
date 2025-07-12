namespace UserService.Core.Entities;

public class UserPermissions:BaseEntity
{
    public string UserId { get; set; }
    public string PermissionName { get; set; }  
    
    public User User { get; set; }
    public Permission Permission { get; set; }
    
}