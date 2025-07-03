namespace TransporterService.Core.Entities;

public enum ContactType
{
    Primary,
    Secondary,
    Emergency,
    Billing,
    Technical
}

public class TransporterContact : BaseEntity
{
    public string TransporterId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public ContactType ContactType { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Transporter Transporter { get; set; } = null!;
}