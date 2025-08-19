namespace Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
public record BackupHealthReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalBackupsChecked { get; set; }
    public int HealthyBackups { get; set; }
    public int UnhealthyBackups { get; set; }
    public double OverallHealthPercentage { get; set; }
    public List<BackupHealthResult> BackupResults { get; set; } = new();
}
