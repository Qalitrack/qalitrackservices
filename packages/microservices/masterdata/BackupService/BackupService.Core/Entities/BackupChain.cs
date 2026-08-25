using System.Text.Json.Serialization;

namespace BackupService.Core.Entities;

public class BackupChain
{
    public int Id { get; init; }
    
    public int MicroserviceId { get; init; }
    public string MicroserviceName { get; init; } = string.Empty;
    public string FullBackupFile { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public List<string> Incrementals { get; init; } = new();    
    
    public string ChainId => $"{MicroserviceName}_{Timestamp:yyyyMMddHHmmss}";
    
    public string? Lsn { get; set; }
    
    public int? Timeline { get; set; }
    
    // NEW: Track the most recent LSN (updated after each incremental backup)
    [JsonPropertyName("lastLsn")]
    public string? LastLsn { get; set; }

    // True for pgBackRest-based physical backups. Their files live inside the
    // pgBackRest repo volume, not as browsable files under the backup directory,
    // so availability is trusted from pgbackrest itself rather than checked with
    // File.Exists like the legacy pg_dump chains below.
    public bool IsPhysical { get; init; }

    // Sizes for physical chains — pgBackRest backups aren't files this service can
    // stat, so the size pgBackRest reported at backup time is recorded here instead.
    // IncrementalSizesBytes stays index-aligned with Incrementals.
    public long FullBackupSizeBytes { get; set; }
    public List<long> IncrementalSizesBytes { get; init; } = new();

    // Index-aligned with Incrementals — each incremental's own completion time, not
    // the base full backup's. Without this, every incremental in a chain reports the
    // same CreatedAt as the full backup, so "restore to latest" (which orders by
    // CreatedAt) can resolve to the full backup instead of the newest incremental.
    public List<DateTime> IncrementalTimestamps { get; init; } = new();
}