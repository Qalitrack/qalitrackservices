namespace TechnicianApi.Core.DTOs.Driver;

public class UpdateDriverProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? IdNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LicenseFrontImageUrl { get; set; }
    public string? LicenseBackImageUrl { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public List<string> LicenseClassIds { get; set; } = new();
}
