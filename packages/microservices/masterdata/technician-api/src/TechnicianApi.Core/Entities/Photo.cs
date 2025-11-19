namespace TechnicianApi.Core.Entities;

public class Photo : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public PhotoType Type { get; set; } = PhotoType.During;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? StorageUrl { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = "image/jpeg";
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Caption { get; set; }

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;
}

public enum PhotoType
{
    Before,
    During,
    After
}
