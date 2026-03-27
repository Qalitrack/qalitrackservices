namespace TechnicianApi.Core.DTOs.Fleet;

public class TruckResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? DriverId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
