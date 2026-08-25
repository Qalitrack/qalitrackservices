namespace BackupService.Core.Dtos;

public class PgBackRestBackupEntry
{
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "full" | "diff" | "incr"
    public long TimestampStop { get; set; } // unix seconds
    public long SizeBytes { get; set; }
    public string? LsnStop { get; set; }
}

public class PgBackRestInfo
{
    public string Stanza { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<PgBackRestBackupEntry> Backups { get; set; } = new();
}

public record PgBackRestBackupOutcome(
    bool Success,
    string Label,
    Enums.BackupType Type,
    DateTime Timestamp,
    long SizeBytes,
    string? Lsn,
    string RawOutput);
