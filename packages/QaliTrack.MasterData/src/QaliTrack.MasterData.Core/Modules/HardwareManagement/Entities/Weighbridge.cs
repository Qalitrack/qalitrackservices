using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.HardwareManagement.Entities;

/// <summary>
/// Weighbridge hardware configuration for cement operations
/// </summary>
public class Weighbridge : BaseEntity
{
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; } // Maximum weight capacity in tons
    public string DirectionCapability { get; set; } = string.Empty; // One_Way, Two_Way
    public string WeighbridgeRole { get; set; } = string.Empty; // Entry, Exit, Both
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? InstallationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<PlcConfiguration> PlcConfigurations { get; set; } = new List<PlcConfiguration>();
    public virtual ICollection<AnprCamera> AnprCameras { get; set; } = new List<AnprCamera>();
}