namespace Messaging.Contracts.Messaging.contracts.Enums;

public enum BackupCommandType
{
    CreateFullBackup,
    CreateIncrementalBackup,
    RestoreBackup,
    ListBackups,
    PreviewRestore,
    VerifyBackup,
    CleanupOldBackups
}