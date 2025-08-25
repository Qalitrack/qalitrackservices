namespace WorkspaceService.Core.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkspaceStatus Status { get; set; } = WorkspaceStatus.Active;
    public string IconUrl { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db";
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int MaxMembers { get; set; } = 100;
    
    // Navigation properties
    public virtual ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public virtual ICollection<WorkspaceInvitation> Invitations { get; set; } = new List<WorkspaceInvitation>();
}

public class WorkspaceMember : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Workspace Workspace { get; set; } = null!;
}

public class WorkspaceInvitation : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public Guid InvitedBy { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? AcceptToken { get; set; }
    
    // Navigation properties
    public virtual Workspace Workspace { get; set; } = null!;
}

public enum WorkspaceStatus
{
    Active,
    Inactive,
    Suspended,
    Archived
}

public enum WorkspaceRole
{
    Member,
    Admin,
    Owner
}

public enum MemberStatus
{
    Active,
    Inactive,
    Suspended
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined,
    Expired,
    Cancelled
}