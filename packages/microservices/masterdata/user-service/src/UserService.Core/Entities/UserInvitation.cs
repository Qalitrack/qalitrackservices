namespace UserService.Core.Entities;

public class UserInvitation : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public string? RoleId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedBy { get; set; }
    public string? Message { get; set; }
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled
}