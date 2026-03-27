namespace TechnicianApi.Core.Entities;

public class SystemSettings : BaseEntity
{
    // Singleton pattern - only one instance should exist
    public bool TesterRegistrationEnabled { get; set; } = false;
    public bool TesterLoginEnabled { get; set; } = false;
    public int LicenseExpiryWarningDays { get; set; } = 30;
    public bool RequireProfilePhoto { get; set; } = true;
    public bool RequireLicenseImages { get; set; } = true;
    public bool RequireIdImages { get; set; } = true;
    public bool AutoApproveProfileUpdates { get; set; } = false;

    // JSON string containing array of user types to auto-approve
    public string? AutoApproveUserTypesJson { get; set; }
}
