namespace TechnicianApi.Core.DTOs.CheckIn;

public class CreateCheckInDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Notes { get; set; }
}
