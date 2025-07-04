namespace CustomerService.Core.Entities;

public class CustomerPreference : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; } = "en";
    public string? PreferredCurrency { get; set; } = "USD";
    public string? TimeZone { get; set; } = "UTC";
    public NotificationSettings NotificationSettings { get; set; } = new();
    public CommunicationPreferences CommunicationPreferences { get; set; } = new();
    public bool AllowMarketing { get; set; } = true;
    public bool AllowSms { get; set; } = true;
    public bool AllowPhone { get; set; } = true;
    public string? PreferredContactTime { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}

public class NotificationSettings
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool InvoiceNotifications { get; set; } = true;
    public bool ContractNotifications { get; set; } = true;
    public bool MaintenanceNotifications { get; set; } = true;
    public bool PromotionalNotifications { get; set; } = false;
}

public class CommunicationPreferences
{
    public string PreferredChannel { get; set; } = "email";
    public string PreferredContactTime { get; set; } = "business_hours";
    public bool OptOutOfMarketing { get; set; } = false;
    public bool OptOutOfSurveys { get; set; } = false;
}