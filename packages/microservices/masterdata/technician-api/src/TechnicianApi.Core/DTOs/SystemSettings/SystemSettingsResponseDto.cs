namespace TechnicianApi.Core.DTOs.SystemSettings;

public class SystemSettingsResponseDto
{
    public string Id { get; set; } = string.Empty;
    public bool TesterRegistrationEnabled { get; set; }
    public bool TesterLoginEnabled { get; set; }
    public int LicenseExpiryWarningDays { get; set; }
    public bool RequireProfilePhoto { get; set; }
    public bool RequireLicenseImages { get; set; }
    public bool RequireIdImages { get; set; }
    public bool AutoApproveProfileUpdates { get; set; }
    public string? AutoApproveUserTypesJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
