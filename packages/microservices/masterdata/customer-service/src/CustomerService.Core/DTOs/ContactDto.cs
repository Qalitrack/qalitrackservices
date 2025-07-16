namespace CustomerService.Core.DTOs;

public class ContactReadDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public bool PreferEmail { get; set; }
    public bool PreferPhone { get; set; }
    public bool PreferSMS { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; }
    public DateTime? LastContactDate { get; set; }
    public string? LastContactMethod { get; set; }
    public string? LastContactNotes { get; set; }
    public int ContactFrequency { get; set; }
    public bool NotifyOnOrderUpdates { get; set; }
    public bool NotifyOnContractRenewals { get; set; }
    public bool NotifyOnPaymentDue { get; set; }
    public bool NotifyOnDeliveryUpdates { get; set; }
    public bool NotifyOnComplianceIssues { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateContactDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string ContactType { get; set; } = "Primary";
    public string Role { get; set; } = "DecisionMaker";
    public bool IsPrimary { get; set; }
    public bool PreferEmail { get; set; } = true;
    public bool PreferPhone { get; set; } = false;
    public bool PreferSMS { get; set; } = false;
    public string? PreferredContactTime { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
    public bool NotifyOnOrderUpdates { get; set; } = true;
    public bool NotifyOnContractRenewals { get; set; } = true;
    public bool NotifyOnPaymentDue { get; set; } = true;
    public bool NotifyOnDeliveryUpdates { get; set; } = true;
    public bool NotifyOnComplianceIssues { get; set; } = true;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? Notes { get; set; }
}

public class UpdateContactDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? Notes { get; set; }
}

public class UpdateContactPreferencesDto
{
    public bool PreferEmail { get; set; }
    public bool PreferPhone { get; set; }
    public bool PreferSMS { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool NotifyOnOrderUpdates { get; set; }
    public bool NotifyOnContractRenewals { get; set; }
    public bool NotifyOnPaymentDue { get; set; }
    public bool NotifyOnDeliveryUpdates { get; set; }
    public bool NotifyOnComplianceIssues { get; set; }
}

public class LogCommunicationDto
{
    public string ContactId { get; set; } = string.Empty;
    public DateTime CommunicationDate { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = "Email";
    public string Direction { get; set; } = "Outbound";
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? InitiatedBy { get; set; }
    public string? ResponseRequired { get; set; }
    public DateTime? ResponseDue { get; set; }
    public string? Notes { get; set; }
}

public class ContactCommunicationReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ContactId { get; set; } = string.Empty;
    public DateTime CommunicationDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? InitiatedBy { get; set; }
    public string? ResponseRequired { get; set; }
    public DateTime? ResponseDue { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ResolveCommunicationDto
{
    public string? Resolution { get; set; }
}