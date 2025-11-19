namespace TechnicianApi.Core.Entities;

public class Technician : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechnicianStatus Status { get; set; } = TechnicianStatus.Active;

    // Navigation properties - Many-to-Many with Assignment
    public virtual ICollection<AssignmentTechnician> AssignmentTechnicians { get; set; } = new List<AssignmentTechnician>();
    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public enum TechnicianStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}