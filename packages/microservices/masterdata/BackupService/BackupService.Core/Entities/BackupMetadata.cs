namespace BackupService.Core.Entities;

public class BackupMetadata
{
    public List<BackupChain> Chains { get; init; } = new();
    public DateTime CreatedAt { get; set; }
    public string? Lsn { get; set; }
    public int? Timeline { get; set; }
    public string? BackupId { get; set; }
    public string? ChainId { get; set; }
    
}