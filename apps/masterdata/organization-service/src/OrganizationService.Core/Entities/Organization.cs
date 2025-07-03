namespace OrganizationService.Core.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Industry { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public string? SubscriptionPlan { get; set; }
    public OrganizationStatus Status { get; set; }
    public string? ParentOrganizationId { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public int MaxUsers { get; set; } = 10;
    public DateTime? SubscriptionExpiryDate { get; set; }

    // Navigation properties
    public Organization? ParentOrganization { get; set; }
    public ICollection<Organization> ChildOrganizations { get; set; } = new List<Organization>();
    public ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();
    public ICollection<OrganizationDepartment> Departments { get; set; } = new List<OrganizationDepartment>();
    public ICollection<OrganizationLocation> Locations { get; set; } = new List<OrganizationLocation>();
    public OrganizationSettings? Settings { get; set; }
    public OrganizationBilling? Billing { get; set; }
    public ICollection<OrganizationSubscription> Subscriptions { get; set; } = new List<OrganizationSubscription>();
}

public enum OrganizationType
{
    Corporate = 0,
    SME = 1,
    Government = 2,
    NonProfit = 3,
    Individual = 4
}

public enum OrganizationStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Trial = 3,
    Expired = 4
}