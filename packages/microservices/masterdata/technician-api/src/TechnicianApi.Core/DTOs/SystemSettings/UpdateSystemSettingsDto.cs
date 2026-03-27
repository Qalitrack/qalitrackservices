namespace TechnicianApi.Core.DTOs.SystemSettings;

public class UpdateSystemSettingsDto
{
    public bool TesterRegistrationEnabled { get; set; }
    public bool TesterLoginEnabled { get; set; }
    public int LicenseExpiryWarningDays { get; set; }
    public bool RequireProfilePhoto { get; set; }
    public bool RequireLicenseImages { get; set; }
    public bool RequireIdImages { get; set; }
    public bool AutoApproveProfileUpdates { get; set; }
    public List<string> AutoApproveUserTypes { get; set; } = new();
}
