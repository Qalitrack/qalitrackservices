namespace TechnicianApi.Core.Entities;

public class Attachment : BaseEntity
{
    // File information
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    // Metadata
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Entity reference
    public string EntityType { get; set; } = string.Empty; // e.g., "PerDiemReturnForm", "AdvanceReturnForm", "Claim"
    public string EntityId { get; set; } = string.Empty;   // The ID of the entity this attachment belongs to
}
