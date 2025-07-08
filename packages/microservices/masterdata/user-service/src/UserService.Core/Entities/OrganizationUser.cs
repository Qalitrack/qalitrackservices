namespace UserService.Core.Entities;

public class OrganizationUser : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public string? EmployeeId { get; set; }
    public string? Position { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public OrganizationUserStatus Status { get; set; } = OrganizationUserStatus.Active;
    public bool IsOwner { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public string? InvitedBy { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}

public enum OrganizationUserStatus
{
    Active,
    Inactive,
    Invited,
    Suspended,
    Left
}