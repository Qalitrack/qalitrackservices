namespace Messaging.Contracts.Messaging.contracts.Enums;

public class BackupOptions
{
    public string Path { get; set; } = "Backups";
    public bool Enabled { get; set; } = true;
    public string? WalArchivePath { get; set; }
    public int? RetentionDays { get; set; }
    
}