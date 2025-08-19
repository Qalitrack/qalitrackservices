/*using BackupService.Core.Contracts.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using BackupService.Core.Interfaces.Services;
using BackupService.Core.Dtos;
using BackupService.Core.Enums;

namespace BackupService.Infrastructure.Messaging.Consumers;

public class BackupCompletedEventConsumer : IConsumer<BackupCompletedEvent>
{
    private readonly IBackupStatusTrackingService _statusTrackingService;
    private readonly ILogger<BackupCompletedEventConsumer> _logger;

    public BackupCompletedEventConsumer(
        IBackupStatusTrackingService statusTrackingService,
        ILogger<BackupCompletedEventConsumer> logger)
    {
        _statusTrackingService = statusTrackingService ?? throw new ArgumentNullException(nameof(statusTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<BackupCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received backup completed event for service {ServiceId}, correlation ID: {CorrelationId}",
            message.ServiceId, message.CorrelationId);

        try
        {
            // Update the status in our tracking system
            var status = await _statusTrackingService.GetBackupStatusAsync(message.CorrelationId);
            if (status == null)
            {
                _logger.LogWarning("Received backup completed event for unknown correlation ID: {CorrelationId}",
                    message.CorrelationId);
                
                // Create a new status entry if we don't have one
                status = new BackupStatusDto
                {
                    CorrelationId = message.CorrelationId,
                    ServiceId = new Guid(message.ServiceId),
                    BackupType = Enum.Parse<BackupType>(message.BackupType),
                    Status = BackupStatus.Completed,
                    RequestTime = message.CompletedAt.AddMinutes(-5), // Estimate request time
                    CompletionTime = message.CompletedAt,
                    BackupId = message.BackupId,
                    BackupPath = message.BackupPath,
                    SizeInBytes = message.SizeInBytes
                };
                
                await _statusTrackingService.TrackBackupStatusAsync(status);
                return;
            }
            
            // Update existing status
            status.Status = BackupStatus.Completed;
            status.CompletionTime = message.CompletedAt;
            status.BackupId = message.BackupId;
            status.BackupPath = message.BackupPath;
            status.SizeInBytes = message.SizeInBytes;
            
            await _statusTrackingService.TrackBackupStatusAsync(status);
            
            _logger.LogInformation("Updated backup status to Completed for correlation ID: {CorrelationId}",
                message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling backup completed event for correlation ID: {CorrelationId}",
                message.CorrelationId);
            
            // Re-throw to trigger retry via MassTransit
            throw;
        }
    }
}*/