namespace CustomerService.Core.Entities;

public class Contact : BaseEntity
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
    public ContactRole Role { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Communication preferences
    public bool PreferEmail { get; set; } = true;
    public bool PreferPhone { get; set; } = false;
    public bool PreferSMS { get; set; } = false;
    public string? PreferredContactTime { get; set; } // e.g., "9AM-5PM", "Weekdays only"
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
    
    // Communication logging
    public DateTime? LastContactDate { get; set; }
    public string? LastContactMethod { get; set; }
    public string? LastContactNotes { get; set; }
    public int ContactFrequency { get; set; } = 0; // Number of times contacted
    
    // Notification preferences
    public bool NotifyOnOrderUpdates { get; set; } = true;
    public bool NotifyOnContractRenewals { get; set; } = true;
    public bool NotifyOnPaymentDue { get; set; } = true;
    public bool NotifyOnDeliveryUpdates { get; set; } = true;
    public bool NotifyOnComplianceIssues { get; set; } = true;
    
    // Additional contact information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<ContactCommunication> Communications { get; set; } = new List<ContactCommunication>();
}

public class ContactCommunication : BaseEntity
{
    public string ContactId { get; set; } = string.Empty;
    public DateTime CommunicationDate { get; set; } = DateTime.UtcNow;
    public CommunicationType Type { get; set; }
    public CommunicationDirection Direction { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? InitiatedBy { get; set; }
    public string? ResponseRequired { get; set; }
    public DateTime? ResponseDue { get; set; }
    public bool IsResolved { get; set; } = false;
    public string? Resolution { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Contact Contact { get; set; } = null!;
}

public enum ContactType
{
    Primary,
    Billing,
    Technical,
    Sales,
    Legal,
    Emergency,
    Operations,
    Compliance
}

public enum ContactRole
{
    DecisionMaker,
    Influencer,
    User,
    TechnicalContact,
    BillingContact,
    LegalContact,
    ComplianceOfficer,
    ProjectManager,
    Coordinator
}

public enum CommunicationType
{
    Email,
    Phone,
    SMS,
    Meeting,
    VideoCall,
    Letter,
    Fax,
    InPerson
}

public enum CommunicationDirection
{
    Inbound,
    Outbound
}