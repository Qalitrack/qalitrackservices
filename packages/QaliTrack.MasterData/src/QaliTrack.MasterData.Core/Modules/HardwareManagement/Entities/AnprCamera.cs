using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.HardwareManagement.Entities;

/// <summary>
/// ANPR (Automatic Number Plate Recognition) camera configuration
/// Specifically designed for HIKVision camera integration
/// </summary>
public class AnprCamera : BaseEntity
{
    public Guid WeighbridgeId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty; // "HIKVision iDS-2CD7A46G0-IZHS"
    public string IpAddress { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty; // Entry, Exit, Overview
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Offline"; // Online, Offline, Maintenance
    public DateTime? LastStatusCheck { get; set; }
    public string? ConfigurationSettings { get; set; } // JSON configuration
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}