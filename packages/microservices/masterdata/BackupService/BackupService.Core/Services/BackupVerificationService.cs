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


public async Task VerifyDatabaseIntegrityAsync(string connectionString, CancellationToken ct = default)
{
    _logger.LogWarning("Database integrity check not fully implemented for {ConnectionString}", connectionString);
    await Task.CompletedTask;
}


async Task<BackupHealthReport> IBackupVerificationService.GenerateBackupHealthReportAsync(List<string> backupPaths, CancellationToken ct)
{
    return await GenerateBackupHealthReportAsync(backupPaths, ct);
}

public async Task<BackupHealthReport> GenerateBackupHealthReportAsync(List<string> backupPaths, CancellationToken ct = default)
{
    var results = new List<BackupHealthResult>();
    int healthy = 0, unhealthy = 0;

    foreach (var path in backupPaths)
    {
        var result = new BackupHealthResult
        {
            BackupPath = path,
            BackupName = Path.GetFileName(path),
            BackupType = BackupType.Full,
            LastModified = _fileSystem.File.Exists(path) ? _fileSystem.FileInfo.New(path).LastWriteTimeUtc : DateTime.UtcNow,
            FileSizeBytes = _fileSystem.File.Exists(path) ? _fileSystem.FileInfo.New(path).Length : 0
        };

        try
        {
            await VerifyBackupIntegrityAsync(path, result.BackupType, ct);
            result.IsHealthy = true;
            result.HealthMessage = "Backup is valid";
            healthy++;
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.HealthMessage = "Backup verification failed";
            result.ErrorDetails = ex.Message;
            unhealthy++;
        }

        results.Add(result);
    }

    return new BackupHealthReport
    {
        GeneratedAt = DateTime.UtcNow,
        TotalBackupsChecked = backupPaths.Count,
        HealthyBackups = healthy,
        UnhealthyBackups = unhealthy,
        OverallHealthPercentage = backupPaths.Count > 0 ? (healthy * 100.0 / backupPaths.Count) : 0,
        BackupResults = results
    };
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