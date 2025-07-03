namespace CustomerService.Core.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public decimal CreditLimit { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? Notes { get; set; }

    public virtual ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    public virtual ICollection<CustomerContract> Contracts { get; set; } = new List<CustomerContract>();
    public virtual ICollection<CustomerLocation> Locations { get; set; } = new List<CustomerLocation>();
    public virtual ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    public virtual CustomerBilling? Billing { get; set; }
    public virtual CustomerCredit? Credit { get; set; }
    public virtual CustomerPreference? Preferences { get; set; }
}

public enum CustomerType
{
    Individual,
    Corporate,
    Government,
    NonProfit
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}