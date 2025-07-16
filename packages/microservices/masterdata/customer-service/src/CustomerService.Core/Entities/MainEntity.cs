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
    
    // Service references - clean separation of concerns
    public string? TransporterId { get; set; }  // References Transporter Service if customer has transport capability
    public string? PreferredTransporterId { get; set; }  // References external Transporter Service
    
    // Business classification
    public bool IsSupplier { get; set; } = false;
    public bool IsBuyer { get; set; } = true;
    
    // Payment and business terms
    public int PaymentTermsDays { get; set; } = 30;
    public string Currency { get; set; } = "KES";

    // Navigation properties
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public virtual ICollection<Order> CustomerOrders { get; set; } = new List<Order>();
    public virtual ICollection<Order> SupplierOrders { get; set; } = new List<Order>();
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