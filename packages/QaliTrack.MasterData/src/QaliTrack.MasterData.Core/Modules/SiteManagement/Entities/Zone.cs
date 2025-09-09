using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.SiteManagement.Entities;

/// <summary>
/// Regional zones for organizing sites (e.g., Coastal Region, Eastern Region, Central Region)
/// </summary>
public class Zone : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
}