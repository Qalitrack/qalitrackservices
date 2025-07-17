namespace UserService.Core.Options;

public class BackupOptions
{
    public const string SectionName = "Backup";
    
    public string Path { get; set; } = "Backups";
    public TimeSpan FullBackupInterval { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan IncrementalBackupInterval { get; set; } = TimeSpan.FromDays(1);
    public int FullBackupRetentionDays { get; set; } = 30;
    public int IncrementalBackupRetentionDays { get; set; } = 7;
    public string[] AdminEmails { get; set; } = Array.Empty<string>();
    public bool Enabled { get; set; } = true;
}
