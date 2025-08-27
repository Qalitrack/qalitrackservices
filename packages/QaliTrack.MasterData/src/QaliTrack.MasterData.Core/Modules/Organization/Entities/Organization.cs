using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Organization.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<OrganizationUser> Users { get; set; } = new List<OrganizationUser>();
    public virtual ICollection<OrganizationLocation> Locations { get; set; } = new List<OrganizationLocation>();
    public virtual OrganizationSettings? Settings { get; set; }
    public virtual OrganizationSubscription? Subscription { get; set; }
}

public class OrganizationUser : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LeftDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class OrganizationLocation : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string LocationType { get; set; } = "Active";
    public bool IsHeadquarters { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class OrganizationSettings : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeFormat { get; set; } = "HH:mm:ss";
    public string Currency { get; set; } = "USD";
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "default";
    public bool NotificationsEnabled { get; set; } = true;
    public string? CustomSettings { get; set; } // JSON storage for additional settings

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class OrganizationSubscription : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PlanType { get; set; } = "Active";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public decimal MonthlyFee { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxLocations { get; set; } = 1;
    public string Features { get; set; } = string.Empty; // JSON array of enabled features
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public enum OrganizationStatus
{
    Active,
    Inactive,
    Suspended,
    Pending,
    Closed
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}

public enum LocationType
{
    Office,
    Warehouse,
    Plant,
    Branch,
    Service
}

public enum SubscriptionPlan
{
    Basic,
    Professional,
    Enterprise,
    Custom
}

public enum SubscriptionStatus
{
    Active,
    Inactive,
    Suspended,
    Expired,
    Trial
}