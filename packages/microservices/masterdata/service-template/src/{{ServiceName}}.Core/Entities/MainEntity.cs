namespace {{ServiceName}}.Core.Entities;

public class {{EntityName}} : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public {{EntityName}}Status Status { get; set; } = {{EntityName}}Status.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<Related{{EntityName}}> Related{{EntityName}}s { get; set; } = new List<Related{{EntityName}}>();
}

public enum {{EntityName}}Status
{
    Active,
    Inactive,
    Suspended,
    Pending
}