using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

/// <summary>
/// Creates full and incremental backups via pgBackRest. Physical backups cover the whole
/// shared Postgres instance (every microservice's schema) in one chain — the microservice
/// parameter identifies who owns/tracks the resulting chain entry, it does not scope what
/// gets backed up.
/// </summary>
public class BackupCreationService : IBackupCreationService
{
    private readonly IMicroserviceRepository _microserviceRepository;
    private readonly IBackupMetadataService _backupMetadataService;
    private readonly IPgBackRestClient _pgBackRestClient;
    private readonly ILogger<BackupCreationService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public BackupCreationService(
        IMicroserviceRepository microserviceRepository,
        IBackupMetadataService backupMetadataService,
        IPgBackRestClient pgBackRestClient,
        ILogger<BackupCreationService> logger)
    {
        _microserviceRepository = microserviceRepository;
        _backupMetadataService = backupMetadataService;
        _pgBackRestClient = pgBackRestClient;
        _logger = logger;
    }

    public async Task<BackupResult> CreateBackupAsync(BackupType backupType, string microservice, CancellationToken ct = default)
    {
        await _operationLock.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();
            var ms = await ValidateMicroserviceAsync(microservice, ct);

            _logger.LogInformation("Creating pgBackRest {BackupType} backup, tracked against {Microservice}", backupType, microservice);

            try
            {
                var outcome = await _pgBackRestClient.BackupAsync(backupType, ct);

                var result = new BackupResult
                {
                    Success = true,
                    Message = $"pgBackRest {backupType} backup completed successfully",
                    BackupId = outcome.Label,
                    FileName = outcome.Label,
                    BackupType = backupType,
                    Timestamp = outcome.Timestamp,
                    FileSizeBytes = outcome.SizeBytes,
                    IsValid = true,
                    ServiceName = microservice,
                    ChainId = outcome.Label,
                    Lsn = outcome.Lsn,
                };

                if (backupType == BackupType.Full)
                {
                    await _backupMetadataService.UpdateMetadataWithFullBackupAsync(result, microservice, ct);
                }
                else
                {
                    await _backupMetadataService.UpdateMetadataWithIncrementalBackupAsync(result, microservice, ct);
                }

                ms.LastBackupAt = DateTime.UtcNow;
                ms.Status = MicroserviceStatus.Active;
                ms.UpdatedAt = DateTime.UtcNow;
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

                _logger.LogInformation("pgBackRest {BackupType} backup completed for {Microservice}: {Label} ({SizeBytes} bytes)",
                    backupType, microservice, outcome.Label, outcome.SizeBytes);

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "pgBackRest backup failed for {Microservice}", microservice);

                ms.Status = MicroserviceStatus.Paused;
                ms.UpdatedAt = DateTime.UtcNow;
                try
                {
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
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update microservice status after backup failure for {Microservice}", microservice);
                }

                throw new InvalidOperationException($"pgBackRest backup failed for {microservice}: {ex.Message}", ex);
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private async Task<Microservice> ValidateMicroserviceAsync(string microservice, CancellationToken ct)
    {
        var ms = await _microserviceRepository.GetMicroserviceAsync(microservice, ct)
            ?? throw new KeyNotFoundException($"Microservice {microservice} not found");

        if (ms.Status != MicroserviceStatus.Active)
        {
            _logger.LogWarning("Microservice {Name} is {Status}, cannot backup", microservice, ms.Status);
            throw new InvalidOperationException($"Microservice {microservice} is not Active");
        }

        return ms;
    }
}
