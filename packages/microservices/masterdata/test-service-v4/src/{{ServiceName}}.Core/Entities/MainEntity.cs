namespace TestServiceV4.Core.Entities;

public class Testentity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TestentityStatus Status { get; set; } = TestentityStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedTestentity> RelatedTestentitys { get; set; } = new List<RelatedTestentity>();
}

public enum TestentityStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}