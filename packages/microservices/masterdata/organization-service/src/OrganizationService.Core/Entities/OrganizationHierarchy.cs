namespace OrganizationService.Core.Entities;

public class OrganizationHierarchy : BaseEntity
{
    public string ParentOrganizationId { get; set; } = string.Empty;
    public string ChildOrganizationId { get; set; } = string.Empty;
    public int Level { get; set; }
    public string HierarchyPath { get; set; } = string.Empty; // e.g., "root/parent/child"
    public string RelationshipType { get; set; } = string.Empty; // e.g., "subsidiary", "division", "branch"

    // Navigation properties
    public Organization ParentOrganization { get; set; } = null!;
    public Organization ChildOrganization { get; set; } = null!;
}