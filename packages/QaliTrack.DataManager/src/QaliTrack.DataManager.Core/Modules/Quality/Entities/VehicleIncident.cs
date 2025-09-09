using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Quality.Entities;

/// <summary>
/// Vehicle incidents related to seal tampering or other violations
/// Prevents vehicles with unresolved incidents from proceeding
/// </summary>
public class VehicleIncident : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Guid? SealRecordId { get; set; }
    public Guid? TransactionId { get; set; }
    public string IncidentType { get; set; } = string.Empty; // SealTamper, WeightDiscrepancy, DocumentMissing
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public DateTime IncidentDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, InvestigationInProgress, Resolved, Closed
    public string? ResolutionNotes { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool BlocksVehicle { get; set; } = true;

    // Navigation properties
    public virtual SealRecord? SealRecord { get; set; }
}