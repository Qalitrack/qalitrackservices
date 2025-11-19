namespace TechnicianApi.Core.DTOs.Photo;

public class CreatePhotoDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string Type { get; set; } = "During";
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? StorageUrl { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = "image/jpeg";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Caption { get; set; }
}
