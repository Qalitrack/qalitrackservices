namespace TechnicianApi.Core.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between Assignment and Technician
/// </summary>
public class AssignmentTechnician : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Assignment Assignment { get; set; } = null!;

    // Additional fields for the relationship (optional)
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public bool IsPrimary { get; set; } = false; // Flag to mark the primary technician if needed
}
