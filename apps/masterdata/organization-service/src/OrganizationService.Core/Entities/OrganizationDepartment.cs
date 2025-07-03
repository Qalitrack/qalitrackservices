namespace OrganizationService.Core.Entities;

public class OrganizationDepartment : BaseEntity
{
    public string OrganizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentDepartmentId { get; set; }
    public string? ManagerId { get; set; } // Reference to OrganizationUser
    public string? Budget { get; set; }
    public string? CostCenter { get; set; }
    public DepartmentStatus Status { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public OrganizationDepartment? ParentDepartment { get; set; }
    public ICollection<OrganizationDepartment> ChildDepartments { get; set; } = new List<OrganizationDepartment>();
    public ICollection<OrganizationUser> Users { get; set; } = new List<OrganizationUser>();
    public OrganizationUser? Manager { get; set; }
}

public enum DepartmentStatus
{
    Active = 0,
    Inactive = 1,
    Archived = 2
}