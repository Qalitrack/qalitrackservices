// using System;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using UserService.Core.Interfaces;
// using UserService.Core.Options;
//
// namespace UserService.Core.Services;
//
// public class BackupVerificationService : IBackupVerificationService
// {
//     private readonly ILogger<BackupVerificationService> _logger;
//     private readonly IOptions<BackupOptions> _options;
//     private readonly IDatabaseBackupService _backupService;
//
//     public BackupVerificationService(
//         IOptions<BackupOptions> options,
//         ILogger<BackupVerificationService> logger,
//         IDatabaseBackupService backupService)
//     {
//         _options = options ?? throw new ArgumentNullException(nameof(options));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
//     }
//
//     public async Task VerifyAllBackupsAsync(CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             var backupPath = _options.Value.Path;
//             if (!Directory.Exists(backupPath))
//             {
//                 _logger.LogWarning("Backup directory does not exist: {BackupPath}", backupPath);
//                 return;
//             }
//
//             var backupFiles = Directory.GetFiles(backupPath, "UserService_*.db")
//                 .OrderBy(f => f)
//                 .ToList();
//
//             _logger.LogInformation("Verifying {Count} backup files in {BackupPath}", 
//                 backupFiles.Count, backupPath);
//
//             foreach (var backupFile in backupFiles)
//             {
//                 if (cancellationToken.IsCancellationRequested)
//                 {
//                     _logger.LogInformation("Backup verification was cancelled");
//                     break;
//                 }
//
//                 try
//                 {
//                     _logger.LogDebug("Verifying backup: {BackupFile}", Path.GetFileName(backupFile));
//                     await _backupService.VerifyBackupIntegrityAsync(backupFile, cancellationToken);
//                     _logger.LogDebug("Backup verified successfully: {BackupFile}", Path.GetFileName(backupFile));
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Failed to verify backup: {BackupFile}", backupFile);
//                     // Move corrupted backups to quarantine
//                     await QuarantineBackupAsync(backupFile, ex, cancellationToken);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error during backup verification");
//             throw;
//         }
//     }
//
//     private async Task QuarantineBackupAsync(string backupPath, Exception error, CancellationToken cancellationToken)
//     {
//         try
//         {
//             var quarantineDir = Path.Combine(_options.Value.Path, "Quarantine");
//             Directory.CreateDirectory(quarantineDir);
//             
//             var fileName = Path.GetFileName(backupPath);
//             var quarantinePath = Path.Combine(quarantineDir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{fileName}");
//             
//             File.Move(backupPath, quarantinePath);
//             
//             // Log the error details to a file in the quarantine directory
//             var errorLogPath = Path.ChangeExtension(quarantinePath, ".error.log");
//             await File.WriteAllTextAsync(
//                 errorLogPath, 
//                 $"Error verifying backup:\n{error}",
//                 cancellationToken);
//                 
//             _logger.LogWarning("Moved corrupted backup to quarantine: {BackupFile}", fileName);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to quarantine corrupted backup: {BackupFile}", backupPath);
//         }
//     }
// }