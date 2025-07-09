namespace TestService.Core.Entities;

public class Item : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedItem> RelatedItems { get; set; } = new List<RelatedItem>();
}

public enum ItemStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}