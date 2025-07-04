namespace UserService.Core.Entities;

public class UserProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public bool IsProfilePublic { get; set; } = false;
    public Dictionary<string, object>? Settings { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}