using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

/// <summary>
/// Restores the shared Postgres instance from a pgBackRest physical backup chain.
/// Backup identifiers here are "pgbackrest:{label}" sentinels produced by
/// BackupMetadataService.GetAvailableBackupsAsync, not filesystem paths — a physical
/// restore brings back everything the stanza covers at once, there is no per-schema
/// restore the way the old pg_dump-based flow allowed.
/// </summary>
public class BackupRestoreService : IBackupRestoreService
{
    public const string PgBackRestPrefix = "pgbackrest:";

    private readonly IMicroserviceRepository _microserviceRepository;
    private readonly IPgBackRestClient _pgBackRestClient;
    private readonly ILogger<BackupRestoreService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public BackupRestoreService(
        IMicroserviceRepository microserviceRepository,
        IPgBackRestClient pgBackRestClient,
        ILogger<BackupRestoreService> logger)
    {
        _microserviceRepository = microserviceRepository;
        _pgBackRestClient = pgBackRestClient;
        _logger = logger;
    }

    public static bool IsPgBackRestIdentifier(string backupFilePath) =>
        !string.IsNullOrWhiteSpace(backupFilePath) && backupFilePath.StartsWith(PgBackRestPrefix, StringComparison.Ordinal);

    public async Task<RestoreResult> RestoreBackupAsync(string microservice, string backupFilePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup identifier must be provided", nameof(backupFilePath));

        if (!IsPgBackRestIdentifier(backupFilePath))
            throw new NotSupportedException(
                $"'{backupFilePath}' is not a pgBackRest backup identifier (expected a '{PgBackRestPrefix}' prefix). " +
                "Logical pg_dump restores are no longer supported.");

        await _operationLock.WaitAsync(ct);
        try
        {
            var ms = await ValidateMicroserviceAsync(microservice, ct);
            var startTime = DateTime.UtcNow;

            _logger.LogWarning(
                "Starting pgBackRest restore for {Microservice} — this restores the entire shared Postgres instance, not just {Microservice}'s schema.",
                microservice, microservice);

            try
            {
                await _pgBackRestClient.RestoreAsync(ct);

                await UpdateMicroserviceStatusAsync(ms, MicroserviceStatus.Active, ct);

                var result = new RestoreResult
                {
                    BackupId = backupFilePath,
                    ServiceName = microservice,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow,
                    IsSuccessful = true,
                    Message = "pgBackRest restore completed successfully. The shared Postgres instance was stopped, restored, and brought back up.",
                    FullBackupUsed = backupFilePath,
                    IncrementalBackupsUsed = new List<string>(),
                    TotalFilesProcessed = 1,
                };

                _logger.LogInformation("Restore completed successfully for {Microservice}. Duration: {Duration}ms",
                    microservice, (DateTime.UtcNow - startTime).TotalMilliseconds);

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "pgBackRest restore failed for {Microservice}", microservice);
                await UpdateMicroserviceStatusAsync(ms, MicroserviceStatus.Paused, ct);
                throw new InvalidOperationException($"Restore failed for {microservice}: {ex.Message}", ex);
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public async Task<RestorePreviewResult> PreviewRestoreAsync(string microservice, string backupFilePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
            throw new ArgumentException("Backup identifier must be provided", nameof(backupFilePath));

        if (!IsPgBackRestIdentifier(backupFilePath))
            throw new NotSupportedException(
                $"'{backupFilePath}' is not a pgBackRest backup identifier (expected a '{PgBackRestPrefix}' prefix).");

        await ValidateMicroserviceAsync(microservice, ct);

        var info = await _pgBackRestClient.GetInfoAsync(ct);
        var fullBackup = info.Backups.FirstOrDefault(b => b.Type == "full");
        var incrementals = info.Backups.Where(b => b.Type != "full").Select(b => b.Label).ToList();
        var latest = info.Backups.OrderByDescending(b => b.TimestampStop).FirstOrDefault();

        var totalSizeBytes = info.Backups.Sum(b => b.SizeBytes);

        return new RestorePreviewResult
        {
            BackupId = backupFilePath,
            ChainId = fullBackup?.Label ?? "N/A",
            FullBackupFile = fullBackup?.Label,
            IncrementalFiles = incrementals,
            TotalFilesToProcess = info.Backups.Count,
            EstimatedRestoreTimeMinutes = CalculateEstimatedRestoreTime(totalSizeBytes),
            RequiredSpaceBytes = totalSizeBytes,
            RestoreToTimestamp = latest != null ? DateTimeOffset.FromUnixTimeSeconds(latest.TimestampStop).UtcDateTime : DateTime.UtcNow,
            ServiceName = microservice,
        };
    }

    private async Task<Microservice> ValidateMicroserviceAsync(string microservice, CancellationToken ct)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        if (ms.Status == MicroserviceStatus.Inactive)
        {
            _logger.LogWarning("Microservice {Name} is Inactive, cannot restore", microservice);
            throw new InvalidOperationException($"Microservice {microservice} is Inactive");
        }

        return ms;
    }

    private async Task UpdateMicroserviceStatusAsync(Microservice ms, MicroserviceStatus status, CancellationToken ct)
    {
        try
        {
            ms.Status = status;
            ms.UpdatedAt = DateTime.UtcNow;
            if (status == MicroserviceStatus.Active)
            {
                ms.LastBackupAt = DateTime.UtcNow;
            }

            await _microserviceRepository.UpdateMicroserviceAsync(
                ms.Name,
                new MicroserviceRequest
                {
                    Name = ms.Name,
                    ConnectionString = ms.ConnectionString,
                    Status = ms.Status,
                    LastBackupAt = ms.LastBackupAt
                },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update microservice status to {Status} for {Microservice}", status, ms.Name);
        }
    }

    private static int CalculateEstimatedRestoreTime(long backupFileSize)
    {
        var sizeBasedMinutes = (int)Math.Ceiling(backupFileSize / (100.0 * 1024 * 1024));
        return Math.Max(2, sizeBasedMinutes);
    }
}
