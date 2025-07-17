using System.Threading.Tasks;

namespace UserService.Core.Interfaces;

public interface IBackupNotificationService
{
    /// <summary>
    /// Sends a notification when a backup is successfully created
    /// </summary>
    /// <param name="backupPath">Path to the created backup file</param>
    /// <param name="sizeInBytes">Size of the backup file in bytes</param>
    /// <returns>Task that completes when the notification is sent</returns>
    Task NotifyBackupSuccessAsync(string backupPath, long sizeInBytes);
    
    /// <summary>
    /// Sends a notification when a backup fails
    /// </summary>
    /// <param name="error">Error message or details about the failure</param>
    /// <returns>Task that completes when the notification is sent</returns>
    Task NotifyBackupFailureAsync(string error);
}
