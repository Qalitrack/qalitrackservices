using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.HardwareManagement.Entities;

/// <summary>
/// PLC (Programmable Logic Controller) configuration for weighbridge automation
/// Specifically designed for Siemens Logo 8 integration
/// </summary>
public class PlcConfiguration : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public string PlcType { get; set; } = string.Empty; // "Siemens Logo 8"
    public string PlcAddress { get; set; } = string.Empty; // IP address or connection string
    public int InputCount { get; set; } = 0;
    public int OutputCount { get; set; } = 0;
    public string ConfigurationData { get; set; } = string.Empty; // JSON configuration
    public bool IsActive { get; set; } = true;
    public DateTime? LastConnectionTest { get; set; }
    public string? ConnectionStatus { get; set; }

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}