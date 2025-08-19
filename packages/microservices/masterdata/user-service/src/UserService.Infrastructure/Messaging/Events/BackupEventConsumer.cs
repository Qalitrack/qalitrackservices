using MassTransit;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces.Messaging;
using UserService.Infrastructure.Backup;

namespace UserService.Infrastructure.Messaging.Events;

public class BackupEventConsumer : IBackupEventConsumer, IConsumer<CrossServiceBackupCommand>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BackupEventConsumer> _logger;
    private readonly IDatabaseBackupService _backupService;

    public BackupEventConsumer(
        IPublishEndpoint publishEndpoint,
        IDatabaseBackupService backupService,
        ILogger<BackupEventConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _backupService = backupService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CrossServiceBackupCommand> context)
    {
        var command = context.Message;
        var orchestrationResult = new BackupOrchestrationResult
        {
            OrchestrationId = command.CommandId,
            CommandType = command.CommandType,
            StartedAt = DateTime.UtcNow,
            ServiceResponses = new Dictionary<string, CrossServiceBackupResponse>()
        };

        try
        {
            _logger.LogInformation(
                "Received CrossServiceBackupCommand - CommandId: {CommandId}, CommandType: {CommandType}",
                command.CommandId, command.CommandType);

            // Log command details
            _logger.LogDebug(
                "Command details - ScheduledAt: {ScheduledAt}, IsCritical: {IsCritical}, ChainId: {ChainId}, BackupFiles: {BackupFilesCount}",
                command.ScheduledAt,
                command.IsCritical,
                command.ChainId ?? "none",
                command.BackupFiles?.Count ?? 0);

            CrossServiceBackupResponse serviceResponse = command.CommandType switch
            {
                BackupCommandType.CreateFullBackup => await HandleCreateFullBackupAsync(command),
                BackupCommandType.CreateIncrementalBackup => await HandleCreateIncrementalBackupAsync(command),
                BackupCommandType.RestoreBackup => await HandleRestoreBackupAsync(command),
                BackupCommandType.ListBackups => await HandleListBackupsAsync(command),
                BackupCommandType.PreviewRestore => await HandlePreviewRestoreAsync(command),
                BackupCommandType.VerifyBackup => await HandleVerifyBackupAsync(command),
                BackupCommandType.CleanupOldBackups => await HandleCleanupOldBackupsAsync(command),
                _ => throw new InvalidOperationException($"Unsupported command type: {command.CommandType}")
            };

            orchestrationResult.ServiceResponses.Add("UserService", serviceResponse);
            orchestrationResult.IsSuccessful = true;
            orchestrationResult.Message = $"Successfully processed {command.CommandType} command";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process CrossServiceBackupCommand - CommandId: {CommandId}", command.CommandId);
            orchestrationResult.IsSuccessful = false;
            orchestrationResult.Message = $"Failed to process {command.CommandType}: {ex.Message}";
            orchestrationResult.ServiceResponses.Add("UserService", new CrossServiceBackupResponse
            {
                CommandId = command.CommandId,
                ServiceName = "UserService",
                IsSuccessful = false,
                Message = ex.Message,
                ErrorDetails = ex.ToString(),
                ProcessedAt = DateTime.UtcNow
            });
        }
        finally
        {
            orchestrationResult.CompletedAt = DateTime.UtcNow;
            await _publishEndpoint.Publish(orchestrationResult, context.CancellationToken);
            _logger.LogInformation("Published BackupOrchestrationResult for CommandId: {CommandId}, Success: {IsSuccessful}",
                command.CommandId, orchestrationResult.IsSuccessful);
        }
    }

    private async Task<CrossServiceBackupResponse> HandleCreateFullBackupAsync(CrossServiceBackupCommand command)
    {
        var result = await _backupService.CreateBackupAsync(BackupType.Full);
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = result.IsValid,
            Message = $"Full backup created: {result.BackupId}",
            ProcessedAt = DateTime.UtcNow,
            BackupResult = result
        };
    }

    private async Task<CrossServiceBackupResponse> HandleCreateIncrementalBackupAsync(CrossServiceBackupCommand command)
    {
        var result = await _backupService.CreateBackupAsync(BackupType.Incremental);
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = result.IsValid,
            Message = $"Incremental backup created: {result.BackupId}",
            ProcessedAt = DateTime.UtcNow,
            BackupResult = result
        };
    }

    private async Task<CrossServiceBackupResponse> HandleRestoreBackupAsync(CrossServiceBackupCommand command)
    {
        var backupId = command.ChainId ?? "latest";
        var result = await _backupService.RestoreBackupAsync(backupId);
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = result.IsSuccessful,
            Message = result.Message,
            ProcessedAt = DateTime.UtcNow,
            RestoreResult = result
        };
    }

    private async Task<CrossServiceBackupResponse> HandleListBackupsAsync(CrossServiceBackupCommand command)
    {
        var backupChains = await _backupService.GetAllBackupChainsAsync();
        var backupFiles = await _backupService.GetAvailableBackupsAsync();
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = true,
            Message = $"Retrieved {backupChains.Count} backup chains and {backupFiles.Count} backup files",
            ProcessedAt = DateTime.UtcNow,
            ChainInfo = backupChains
        };
    }

    private async Task<CrossServiceBackupResponse> HandlePreviewRestoreAsync(CrossServiceBackupCommand command)
    {
        var backupId = command.ChainId ?? "latest";
        var result = await _backupService.PreviewRestoreAsync(backupId);
        var chainInfo = await _backupService.GetBackupChainInfoAsync(backupId);
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = true,
            Message = $"Restore preview generated for backup: {backupId}",
            ProcessedAt = DateTime.UtcNow,
            RestorePreviewResult = result,
            ChainInfo = chainInfo != null ? new List<BackupChainInfo> { chainInfo } : new List<BackupChainInfo>()
        };
    }

    private async Task<CrossServiceBackupResponse> HandleVerifyBackupAsync(CrossServiceBackupCommand command)
    {
        var report = await _backupService.GenerateHealthReportAsync();
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = report.UnhealthyBackups == 0,
            Message = $"Backup health report generated: {report.HealthyBackups}/{report.TotalBackupsChecked} healthy ({report.OverallHealthPercentage:F1}%)",
            ProcessedAt = DateTime.UtcNow,
            HealthReport = report,
            ChainInfo = (await _backupService.GetAllBackupChainsAsync()).Where(c => report.BackupResults.Any(r => r.BackupName == c.FullBackupFile || c.IncrementalFiles.Contains(r.BackupName))).ToList()
        };
    }

    private async Task<CrossServiceBackupResponse> HandleCleanupOldBackupsAsync(CrossServiceBackupCommand command)
    {
        await _backupService.ValidateAllBackupsAsync();
        var backupChains = await _backupService.GetAllBackupChainsAsync();
        return new CrossServiceBackupResponse
        {
            CommandId = command.CommandId,
            ServiceName = "UserService",
            IsSuccessful = true,
            Message = $"Old backups cleaned up successfully, {backupChains.Count} chains remain",
            ProcessedAt = DateTime.UtcNow,
            ChainInfo = backupChains
        };
    }
}