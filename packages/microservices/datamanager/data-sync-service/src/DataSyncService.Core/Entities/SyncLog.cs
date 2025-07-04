using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Entities;

public class SyncLog : BaseEntity
{
    public string LogId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // Info, Warning, Error, Debug
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public DateTime LogTimestamp { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public string? MetadataJson { get; set; }
    public string? CorrelationId { get; set; }
    
    // Navigation properties
    public virtual SyncSession SyncSession { get; set; } = null!;
}