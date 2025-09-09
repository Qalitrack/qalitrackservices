using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Quality.Entities;

/// <summary>
/// Container seal tracking for tamper detection
/// </summary>
public class SealRecord : BaseEntity
{
    public Guid TransactionId { get; set; }
    public Guid VehicleId { get; set; }
    public string SealNumber { get; set; } = string.Empty;
    public string SealType { get; set; } = string.Empty; // Plastic, Metal, Electronic
    public DateTime SealAppliedAt { get; set; }
    public string SealAppliedBy { get; set; } = string.Empty;
    public DateTime? SealCheckedAt { get; set; }
    public string? SealCheckedBy { get; set; }
    public string SealStatus { get; set; } = "Intact"; // Intact, Tampered, Missing, Damaged
    public string? TamperDescription { get; set; }
    public bool RequiresInvestigation { get; set; } = false;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<VehicleIncident> VehicleIncidents { get; set; } = new List<VehicleIncident>();
}