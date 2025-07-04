namespace CustomerService.Core.Entities;

public class CustomerContact : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public ContactType ContactType { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Customer Customer { get; set; } = null!;
}

public enum ContactType
{
    Primary,
    Billing,
    Technical,
    Sales,
    Legal,
    Emergency
}