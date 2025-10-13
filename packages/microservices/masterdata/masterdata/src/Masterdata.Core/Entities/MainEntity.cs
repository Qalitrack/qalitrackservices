namespace Masterdata.Core.Entities;

public class Base : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BaseStatus Status { get; set; } = BaseStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedBase> RelatedBases { get; set; } = new List<RelatedBase>();
}

public enum BaseStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}