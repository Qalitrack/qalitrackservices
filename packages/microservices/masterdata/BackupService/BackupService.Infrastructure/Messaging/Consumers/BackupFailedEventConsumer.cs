/*using BackupService.Core.Contracts.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using BackupService.Core.Interfaces.Services;
using BackupService.Core.Dtos;
using BackupService.Core.Enums;

namespace BackupService.Infrastructure.Messaging.Consumers;

public class BackupFailedEventConsumer : IConsumer<BackupFailedEvent>
{
    private readonly IBackupStatusTrackingService _statusTrackingService;
    private readonly ILogger<BackupFailedEventConsumer> _logger;

    public BackupFailedEventConsumer(
        IBackupStatusTrackingService statusTrackingService,
        ILogger<BackupFailedEventConsumer> logger)
    {
        _statusTrackingService = statusTrackingService ?? throw new ArgumentNullException(nameof(statusTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<BackupFailedEvent> context)
    {
        var message = context.Message;
        _logger.LogWarning("Received backup failed event for service {ServiceId}, correlation ID: {CorrelationId}, reason: {Reason}",
            message.ServiceId, message.CorrelationId, message.Reason);

        try
        {
            // Update the status in our tracking system
            var status = await _statusTrackingService.GetBackupStatusAsync(message.CorrelationId);
            if (status == null)
            {
                _logger.LogWarning("Received backup failed event for unknown correlation ID: {CorrelationId}",
                    message.CorrelationId);
                
                // Create a new status entry if we don't have one
                status = new BackupService.Core.Dtos.BackupStatusDto
                {
                    CorrelationId = message.CorrelationId,
                    ServiceId = new Guid(message.ServiceId),
                    BackupType = Enum.Parse<BackupType>(message.BackupType),
                    Status = BackupStatus.Failed,
                    RequestTime = message.FailedAt.AddMinutes(-5), // Estimate request time
                    CompletionTime = message.FailedAt,
                    ErrorMessage = message.Reason
                };
                
                await _statusTrackingService.TrackBackupStatusAsync(status);
                return;
            }
            
            // Update existing status
            status.Status = BackupStatus.Failed;
            status.CompletionTime = message.FailedAt;
            status.ErrorMessage = message.Reason;
            
            await _statusTrackingService.TrackBackupStatusAsync(status);
            
            _logger.LogWarning("Updated backup status to Failed for correlation ID: {CorrelationId}",
                message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling backup failed event for correlation ID: {CorrelationId}",
                message.CorrelationId);
            
            // Re-throw to trigger retry via MassTransit
            throw;
        }
    }
}*/