namespace BackupService.Core.Dtos;

public class RestoreBackupRequest
{
    /// <summary>
    /// The name of the microservice to restore
    /// </summary>
    public string Microservice { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the backup to restore (defaults to 'latest')
    /// </summary>
    public string BackupId { get; set; } = "latest";
}