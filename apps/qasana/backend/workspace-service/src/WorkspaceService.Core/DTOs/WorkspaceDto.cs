namespace WorkspaceService.Core.DTOs;

public class WorkspaceReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<WorkspaceMemberDto> Members { get; set; } = new();
    public List<WorkspaceInvitationDto> PendingInvitations { get; set; } = new();
}

public class CreateWorkspaceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db";
    public int MaxMembers { get; set; } = 100;
}

public class UpdateWorkspaceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
}

public class WorkspaceMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class WorkspaceInvitationDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class InviteMemberDto
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}

public class UpdateMemberRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class AcceptInvitationDto
{
    public string Token { get; set; } = string.Empty;
}