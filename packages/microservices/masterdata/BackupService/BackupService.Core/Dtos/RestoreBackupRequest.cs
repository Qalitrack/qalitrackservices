namespace BackupService.Core.Dtos;

public class RestoreBackupRequest
{
    public string Microservice { get; set; } = string.Empty;

    private string _backupSourcePath = string.Empty;
    public string BackupSourcePath
    {
        get => _backupSourcePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || System.IO.Path.GetFileName(value) == string.Empty)
                throw new ArgumentException("BackupSourcePath must be a full path including the file name.");
            _backupSourcePath = value;
        }
    }

    public string BackupId { get; set; } = "latest";
}