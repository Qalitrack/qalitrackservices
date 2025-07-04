using CustomerService.Core.Entities;

namespace CustomerService.Core.DTOs;

public class CustomerPreferenceDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? TimeZone { get; set; }
    public NotificationSettingsDto NotificationSettings { get; set; } = new();
    public CommunicationPreferencesDto CommunicationPreferences { get; set; } = new();
    public bool AllowMarketing { get; set; }
    public bool AllowSms { get; set; }
    public bool AllowPhone { get; set; }
    public string? PreferredContactTime { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool InvoiceNotifications { get; set; }
    public bool ContractNotifications { get; set; }
    public bool MaintenanceNotifications { get; set; }
    public bool PromotionalNotifications { get; set; }
}

public class CommunicationPreferencesDto
{
    public string PreferredChannel { get; set; } = string.Empty;
    public string PreferredContactTime { get; set; } = string.Empty;
    public bool OptOutOfMarketing { get; set; }
    public bool OptOutOfSurveys { get; set; }
}