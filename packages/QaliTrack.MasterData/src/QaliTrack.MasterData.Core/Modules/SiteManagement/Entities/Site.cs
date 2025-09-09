using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.SiteManagement.Entities;

/// <summary>
/// Physical business locations (plants, warehouses, terminals, branches)
/// Replaces the multi-tenant OrganizationLocation with single-organization, multi-site approach
/// </summary>
public class Site : BaseEntity
{
    public Guid LocationTypeId { get; set; }
    public Guid? ZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual LocationType LocationType { get; set; } = null!;
    public virtual Zone? Zone { get; set; }
}