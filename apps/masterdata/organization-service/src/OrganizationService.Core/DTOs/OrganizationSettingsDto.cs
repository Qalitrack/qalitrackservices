namespace OrganizationService.Core.DTOs;

public class OrganizationSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeFormat { get; set; } = "HH:mm:ss";
    public string Language { get; set; } = "en";
    public string Country { get; set; } = string.Empty;
    public bool EnableMultiTenant { get; set; } = true;
    public bool EnableAuditLog { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
    public bool EnableSMS { get; set; } = false;
    public bool EnableEmail { get; set; } = true;
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmsProvider { get; set; }
    public string? LogoUrl { get; set; }
    public string? BrandColor { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
}

public class UpdateOrganizationSettingsRequest
{
    public string Timezone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeFormat { get; set; } = "HH:mm:ss";
    public string Language { get; set; } = "en";
    public string Country { get; set; } = string.Empty;
    public bool EnableMultiTenant { get; set; } = true;
    public bool EnableAuditLog { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
    public bool EnableSMS { get; set; } = false;
    public bool EnableEmail { get; set; } = true;
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
    public string? SmsProvider { get; set; }
    public string? SmsApiKey { get; set; }
    public string? LogoUrl { get; set; }
    public string? BrandColor { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
}