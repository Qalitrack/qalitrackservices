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
    
}