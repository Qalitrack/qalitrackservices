namespace TechnicianApi.Core.DTOs.CheckIn;

public class CheckInResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CheckInTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
