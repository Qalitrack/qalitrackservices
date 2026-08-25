using System.Diagnostics;
using System.IO.Abstractions;
using BackupService.Core.Dtos;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;
public class BackupVerificationService : IBackupVerificationService { private readonly IFileSystem _fileSystem; private readonly ILogger _logger;

public BackupVerificationService(IFileSystem fileSystem, ILogger<BackupVerificationService> logger)
{
    _fileSystem = fileSystem;
    _logger = logger;
}

public async Task VerifyBackupIntegrityAsync(string backupPath, BackupType backupType, CancellationToken ct = default)
{
    if (BackupRestoreService.IsPgBackRestIdentifier(backupPath))
    {
        // pgBackRest verifies checksums as part of every backup/archive-push by default —
        // there's no separate file for pg_restore --schema-only to sanity-check here.
        _logger.LogInformation("Skipping file-based verification for pgBackRest backup {BackupPath} — integrity is checked by pgBackRest itself", backupPath);
        return;
    }

    if (!_fileSystem.File.Exists(backupPath))
    {
        _logger.LogError("Backup file {BackupPath} not found", backupPath);
        throw new FileNotFoundException($"Backup file {backupPath} not found");
    }

    try
    {
        var pgRestorePath = "pg_restore";
        var arguments = $"--schema-only --file={backupPath}";
        await ExecuteCommandAsync(pgRestorePath, arguments, ct);
        _logger.LogInformation("Verified integrity of {BackupType} backup: {BackupPath}", backupType, backupPath);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Integrity verification failed for {BackupPath}", backupPath);
        throw new InvalidOperationException($"Verification failed for {backupPath}: {ex.Message}", ex);
    }
}


private async Task ExecuteCommandAsync(string command, string arguments, CancellationToken ct)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();
    var error = await process.StandardError.ReadToEndAsync(ct);
    await process.WaitForExitAsync(ct);

    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Command {command} failed: {error}");
}

}