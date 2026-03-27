namespace TechnicianApi.Core.DTOs.Driver;

public class DriverActivityResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string? ActivityData { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
