namespace TechnicianApi.Core.Entities;

public class Technician : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechnicianStatus Status { get; set; } = TechnicianStatus.Active;
    
    // TODO: Add domain-specific properties here
    // Example properties (remove/modify as needed):
    // public string Code { get; set; } = string.Empty;
    // public DateTime? ValidFrom { get; set; }
    // public DateTime? ValidTo { get; set; }
    
    // Navigation properties (modify as needed)
    // public virtual ICollection<RelatedTechnician> RelatedTechnicians { get; set; } = new List<RelatedTechnician>();
}

public enum TechnicianStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}