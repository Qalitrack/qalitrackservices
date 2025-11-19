namespace TechnicianApi.Core.DTOs.Photo;

public class PhotoResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? StorageUrl { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
}
