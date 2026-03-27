namespace TechnicianApi.Core.DTOs.Driver;

public class UpdateDriverDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
}
