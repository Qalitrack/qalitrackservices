namespace OrganizationService.Core.Entities;

public class OrganizationSubscription : BaseEntity
{
    public string OrganizationId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public string PlanCurrency { get; set; } = "USD";
    public SubscriptionPeriod BillingPeriod { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; } = true;
    public int MaxUsers { get; set; }
    public int MaxStorage { get; set; } // in GB
    public Dictionary<string, int> FeatureLimits { get; set; } = new Dictionary<string, int>();
    public List<string> Features { get; set; } = new List<string>();
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
}

public enum SubscriptionPeriod
{
    Monthly = 0,
    Quarterly = 1,
    Annually = 2,
    Lifetime = 3
}

public enum SubscriptionStatus
{
    Active = 0,
    Trial = 1,
    Expired = 2,
    Cancelled = 3,
    Suspended = 4,
    PendingCancellation = 5
}