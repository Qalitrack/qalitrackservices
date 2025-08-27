
namespace BackupService.Core.Dtos;

public class BackupHealthReport
{
    public DateTime GeneratedAt { get; init; }
    public int TotalBackupsChecked { get; init; }
    public int HealthyBackups { get; init; }
    public int UnhealthyBackups { get; init; }
    public double OverallHealthPercentage { get; init; }
    public List<BackupHealthResult> BackupResults { get; init; } = new();
}