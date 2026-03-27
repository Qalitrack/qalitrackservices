using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class DriverProfileChange : BaseEntity
{
    public string? OldProfileId { get; set; }

    [Required]
    public string NewProfileId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [Required]
    public ProfileChangeType ChangeType { get; set; }

    // Navigation properties
    public virtual DriverProfile? OldProfile { get; set; }
    public virtual DriverProfile NewProfile { get; set; } = null!;
}

public enum ProfileChangeType
{
    Added,
    Modified,
    Removed
}
