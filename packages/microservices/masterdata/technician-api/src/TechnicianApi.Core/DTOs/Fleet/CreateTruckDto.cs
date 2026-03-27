namespace TechnicianApi.Core.DTOs.Fleet;

public class CreateTruckDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? DriverId { get; set; }
}
