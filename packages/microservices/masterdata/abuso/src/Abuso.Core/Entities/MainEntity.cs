namespace Abuso.Core.Entities;

public class Abuso : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbusoStatus Status { get; set; } = AbusoStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedAbuso> RelatedAbusos { get; set; } = new List<RelatedAbuso>();
}

public enum AbusoStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}