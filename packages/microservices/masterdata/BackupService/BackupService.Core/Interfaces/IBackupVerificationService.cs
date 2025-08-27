using BackupService.Core.Dtos;
using BackupService.Core.Enums;

namespace BackupService.Core.Interfaces;


public interface IBackupVerificationService
{
    Task VerifyBackupIntegrityAsync(string backupPath, BackupType backupType, CancellationToken ct = default);

}