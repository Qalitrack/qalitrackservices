namespace UserService.Core.Entities;

public class BackupChain
{
    public string FullBackupFile { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<string> Incrementals { get; set; } = new List<string>();
}