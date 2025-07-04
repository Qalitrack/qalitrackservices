namespace UserService.Core.Entities;

public class UserSession : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastAccessedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}