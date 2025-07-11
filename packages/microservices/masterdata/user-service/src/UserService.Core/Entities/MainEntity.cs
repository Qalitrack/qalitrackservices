namespace UserService.Core.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedUser> RelatedUsers { get; set; } = new List<RelatedUser>();
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}