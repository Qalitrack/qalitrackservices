using System.IO.Compression;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Buffers;

namespace UserService.Infrastructure.Backup;

public class BackupVerificationService : IBackupVerificationService
{
    private readonly ILogger<BackupVerificationService> _logger;
    private const int VerificationChunkSize = 1024 * 1024; // 1MB for verification

    public BackupVerificationService(ILogger<BackupVerificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task VerifyBackupIntegrityAsync(string backupPath, BackupType backupType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupPath))
            throw new ArgumentException("Backup path cannot be empty", nameof(backupPath));

        try
        {
            var fileInfo = ValidateBackupFile(backupPath);
            await VerifyArchiveIntegrityAsync(backupPath, cancellationToken);

            if (backupType == BackupType.Full)
            {
                await VerifyFullBackupStructureAsync(backupPath, cancellationToken);
            }
            else
            {
                await VerifyIncrementalBackupStructureAsync(backupPath, cancellationToken);
            }

            _logger.LogDebug("Backup integrity verified: {BackupPath} ({FileSize} bytes, {BackupType})",
                backupPath, fileInfo.Length, backupType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Backup integrity verification failed: {BackupPath}", backupPath);
            throw new InvalidOperationException($"Backup integrity verification failed for {backupPath}", ex);
        }
    }

    private FileInfo ValidateBackupFile(string backupPath)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException($"Backup file not found: {backupPath}");

        var fileInfo = new FileInfo(backupPath);
        if (fileInfo.Length == 0)
            throw new InvalidDataException($"Backup file is empty: {backupPath}");

        return fileInfo;
    }

    private async Task VerifyArchiveIntegrityAsync(string backupPath, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        var buffer = ArrayPool<byte>.Shared.Rent(1024);
        try
        {
            var totalRead = 0;
            int bytesRead;

            while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                totalRead += bytesRead;
                if (totalRead > VerificationChunkSize) break;
            }

            if (totalRead == 0)
                throw new InvalidDataException($"Backup archive appears corrupted: {backupPath}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task VerifyDatabaseIntegrityAsync(string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await VerifyBasicDatabaseConnectivityAsync(connection, cancellationToken);
            await VerifyDatabaseSizeAsync(connection, cancellationToken);
            await RunDatabaseConsistencyChecksAsync(connection, cancellationToken);

            _logger.LogInformation("Database integrity verified");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Database integrity verification failed");
            throw new InvalidOperationException("Database integrity verification failed", ex);
        }
    }

    private static async Task VerifyBasicDatabaseConnectivityAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var pingCommand = new NpgsqlCommand("SELECT 1", connection);
        await pingCommand.ExecuteScalarAsync(cancellationToken);
    }

    private async Task VerifyDatabaseSizeAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var sizeCommand = new NpgsqlCommand("SELECT pg_database_size(current_database())", connection);
        var dbSize = await sizeCommand.ExecuteScalarAsync(cancellationToken);

        if (dbSize == null || Convert.ToInt64(dbSize) <= 0)
            throw new InvalidOperationException("Database appears to be corrupted or empty");
    }

    public async Task<bool> VerifyBackupChainIntegrityAsync(string fullBackupPath, List<string> incrementalPaths,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullBackupPath))
            throw new ArgumentException("Full backup path cannot be empty", nameof(fullBackupPath));

        try
        {
            await VerifyBackupIntegrityAsync(fullBackupPath, BackupType.Full, cancellationToken);

            foreach (var incPath in incrementalPaths)
            {
                if (string.IsNullOrWhiteSpace(incPath))
                {
                    _logger.LogWarning("Empty incremental backup path encountered in backup chain");
                    continue;
                }
                await VerifyBackupIntegrityAsync(incPath, BackupType.Incremental, cancellationToken);
            }

            await VerifyTemporalConsistencyAsync(fullBackupPath, incrementalPaths, cancellationToken);

            _logger.LogInformation("Backup chain integrity verified: {FullBackup} with {IncrementalCount} incrementals",
                Path.GetFileName(fullBackupPath), incrementalPaths.Count);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Backup chain integrity verification failed");
            return false;
        }
    }

    public async Task<BackupHealthReport> GenerateBackupHealthReportAsync(List<string> backupPaths,
        CancellationToken cancellationToken = default)
    {
        if (backupPaths == null)
            throw new ArgumentNullException(nameof(backupPaths));

        var report = new BackupHealthReport
        {
            GeneratedAt = DateTime.UtcNow,
            TotalBackupsChecked = backupPaths.Count,
            BackupResults = new List<BackupHealthResult>()
        };

        foreach (var backupPath in backupPaths)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                _logger.LogWarning("Empty backup path encountered in health report generation");
                continue;
            }

            var result = new BackupHealthResult
            {
                BackupPath = backupPath,
                BackupName = Path.GetFileName(backupPath),
                BackupType = DetermineBackupType(backupPath)
            };

            try
            {
                await VerifyBackupIntegrityAsync(backupPath, result.BackupType, cancellationToken);

                var fileInfo = new FileInfo(backupPath);
                result.IsHealthy = true;
                result.FileSizeBytes = fileInfo.Length;
                result.LastModified = fileInfo.LastWriteTimeUtc;
                result.HealthMessage = "Backup is healthy and accessible";

                report.HealthyBackups++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.IsHealthy = false;
                result.HealthMessage = $"Backup verification failed: {ex.Message}";
                result.ErrorDetails = ex.ToString();

                report.UnhealthyBackups++;
            }

            report.BackupResults.Add(result);
        }

        report.OverallHealthPercentage = report.TotalBackupsChecked > 0
            ? (double)report.HealthyBackups / report.TotalBackupsChecked * 100
            : 0;

        _logger.LogInformation(
            "Generated backup health report: {HealthyCount}/{TotalCount} backups healthy ({HealthPercentage:F1}%)",
            report.HealthyBackups, report.TotalBackupsChecked, report.OverallHealthPercentage);

        return report;
    }

    private async Task VerifyFullBackupStructureAsync(string backupPath, CancellationToken cancellationToken)
    {
        var essentialFiles = new[] { "PG_VERSION", "postgresql.conf", "pg_hba.conf" };
        var foundFiles = new List<string>();

        try
        {
            using var fileStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            var content = new System.Text.StringBuilder();
            int bytesRead;

            while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0 &&
                   content.Length < VerificationChunkSize)
            {
                content.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            ArrayPool<byte>.Shared.Return(buffer);

            foreach (var file in essentialFiles)
            {
                if (content.ToString().Contains(file))
                {
                    foundFiles.Add(file);
                }
            }

            if (foundFiles.Count < essentialFiles.Length / 2)
            {
                _logger.LogWarning(
                    "Full backup may be incomplete - only found {FoundCount}/{ExpectedCount} essential files",
                    foundFiles.Count, essentialFiles.Length);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not fully verify full backup structure for {BackupPath}", backupPath);
        }
    }

    private async Task VerifyIncrementalBackupStructureAsync(string backupPath, CancellationToken cancellationToken)
    {
        try
        {
            using var fileStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

            var buffer = ArrayPool<byte>.Shared.Rent(1024);
            var content = new System.Text.StringBuilder();
            int bytesRead;

            while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0 &&
                   content.Length < VerificationChunkSize / 2)
            {
                content.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            ArrayPool<byte>.Shared.Return(buffer);

            if (!content.ToString().Contains(".wal") && !content.ToString().Contains("pg_wal"))
            {
                _logger.LogWarning("Incremental backup may not contain WAL files: {BackupPath}", backupPath);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not fully verify incremental backup structure for {BackupPath}", backupPath);
        }
    }

    private async Task RunDatabaseConsistencyChecksAsync(NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        var checks = new[]
        {
            ("SELECT COUNT(*) FROM pg_stat_activity", "Active connections check"),
            ("SELECT pg_is_in_recovery()", "Recovery status check"),
            ("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'", "Table count check")
        };

        foreach (var (query, description) in checks)
        {
            try
            {
                await using var command = new NpgsqlCommand(query, connection);
                var result = await command.ExecuteScalarAsync(cancellationToken);
                _logger.LogDebug("{Description}: {Result}", description, result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Database consistency check failed: {Description}", description);
            }
        }
    }

    private async Task VerifyTemporalConsistencyAsync(string fullBackupPath, List<string> incrementalPaths,
        CancellationToken cancellationToken)
    {
        if (!incrementalPaths.Any()) return;

        try
        {
            var fullBackupTime = new FileInfo(fullBackupPath).CreationTimeUtc;
            var previousTime = fullBackupTime;

            foreach (var incPath in incrementalPaths.OrderBy(p => new FileInfo(p).CreationTimeUtc))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var incTime = new FileInfo(incPath).CreationTimeUtc;

                if (incTime <= previousTime)
                {
                    _logger.LogWarning(
                        "Temporal inconsistency detected: {IncrementalBackup} ({IncTime}) is not newer than previous backup ({PrevTime})",
                        Path.GetFileName(incPath), incTime, previousTime);
                }

                previousTime = incTime;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Could not verify temporal consistency of backup chain");
        }
    }

    private BackupType DetermineBackupType(string backupPath)
    {
        var fileName = Path.GetFileName(backupPath);
        return fileName.Contains("_full_", StringComparison.OrdinalIgnoreCase) 
            ? BackupType.Full 
            : BackupType.Incremental;
    }
}