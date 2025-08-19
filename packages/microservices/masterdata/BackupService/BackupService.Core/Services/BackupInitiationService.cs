using System.Collections.Concurrent;
using BackupService.Core.Entities;
using BackupService.Core.Interfaces;
using MassTransit;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services
{
    public class BackupInitiationService : IBackupInitiationService, IConsumer<BackupOrchestrationResult>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<CrossServiceBackupCommand> _requestClient;
        private readonly ILogger<BackupInitiationService> _logger;
        private readonly IBackupOperationRepository _backupOperationRepository;
        
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<BackupOrchestrationResult>> _pendingCommands = new();

        public BackupInitiationService(
            IPublishEndpoint publishEndpoint,
            IRequestClient<CrossServiceBackupCommand> requestClient,
            ILogger<BackupInitiationService> logger,
            IBackupOperationRepository backupOperationRepository)
        {
            _publishEndpoint = publishEndpoint;
            _requestClient = requestClient;
            _logger = logger;
            _backupOperationRepository = backupOperationRepository;
        }

        public async Task<string> InitiateBackupCommandAsync(BackupCommandType commandType, bool isCritical = false, string? chainId = null)
        {
            var commandId = Guid.NewGuid().ToString();
            var command = new CrossServiceBackupCommand
            {
                CommandId = commandId,
                CommandType = commandType,
                IsCritical = isCritical,
                ChainId = chainId,
                ScheduledAt = DateTime.UtcNow
            };

            var completionSource = new TaskCompletionSource<BackupOrchestrationResult>();
            _pendingCommands.TryAdd(commandId, completionSource);

            try
            {
                var operationRecord = new BackupOperationRecord
                {
                    CommandId = commandId,
                    CommandType = commandType.ToString(),
                    InitiatedAt = DateTime.UtcNow,
                    Status = "Initiated",
                    IsCritical = isCritical,
                    ChainId = chainId
                };

                await _backupOperationRepository.AddAsync(operationRecord);
                await _publishEndpoint.Publish(command);
                
                _logger.LogInformation("Initiated backup command {CommandType} with ID {CommandId}", commandType, commandId);
                
                return commandId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate backup command {CommandType}", commandType);
                _pendingCommands.TryRemove(commandId, out _);
                throw;
            }
        }

        public async Task<BackupOrchestrationResult?> GetCommandResultAsync(string commandId, TimeSpan? timeout = null)
        {
            if (_pendingCommands.TryGetValue(commandId, out var completionSource))
            {
                try
                {
                    return timeout.HasValue 
                        ? await completionSource.Task.WaitAsync(timeout.Value) 
                        : await completionSource.Task;
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Timeout waiting for result of command {CommandId}", commandId);
                    return null;
                }
            }

            var operation = await _backupOperationRepository.GetByCommandIdAsync(commandId);
            if (operation != null && operation.CompletedAt.HasValue)
            {
                return new BackupOrchestrationResult
                {
                    OrchestrationId = operation.CommandId,
                    CommandType = Enum.Parse<BackupCommandType>(operation.CommandType),
                    StartedAt = operation.InitiatedAt,
                    CompletedAt = operation.CompletedAt.Value,
                    IsSuccessful = operation.IsSuccessful,
                    Message = operation.ResultMessage,
                    ServiceResponses = operation.ServiceResponses?
                        .ToDictionary(r => r.ServiceName, r => new CrossServiceBackupResponse
                        {
                            CommandId = r.CommandId,
                            ServiceName = r.ServiceName,
                            IsSuccessful = r.IsSuccessful,
                            Message = r.Message,
                            ErrorDetails = r.ErrorDetails,
                            ProcessedAt = r.ProcessedAt
                        }) ?? new Dictionary<string, CrossServiceBackupResponse>()
                };
            }

            return null;
        }

        public async Task<List<BackupOperationRecord>> GetRecentBackupOperationsAsync(int count = 10)
        {
            return await _backupOperationRepository.GetRecentAsync(count);
        }

        public async Task Consume(ConsumeContext<BackupOrchestrationResult> context)
        {
            var result = context.Message;
            
            _logger.LogInformation("Received orchestration result for command {CommandId}, success: {IsSuccessful}", 
                result.OrchestrationId, result.IsSuccessful);

            if (_pendingCommands.TryRemove(result.OrchestrationId, out var completionSource))
            {
                completionSource.TrySetResult(result);
            }

            var operation = await _backupOperationRepository.GetByCommandIdAsync(result.OrchestrationId);
            if (operation != null)
            {
                operation.CompletedAt = result.CompletedAt;
                operation.IsSuccessful = result.IsSuccessful;
                operation.Status = result.IsSuccessful ? "Completed" : "Failed";
                operation.ResultMessage = result.Message;
                operation.ServiceResponses = result.ServiceResponses?
                    .Select(r => new BackupServiceResponseRecord
                    {
                        CommandId = r.Value.CommandId,
                        ServiceName = r.Value.ServiceName,
                        IsSuccessful = r.Value.IsSuccessful,
                        Message = r.Value.Message,
                        ErrorDetails = r.Value.ErrorDetails,
                        ProcessedAt = r.Value.ProcessedAt
                    })
                    .ToList();

                await _backupOperationRepository.UpdateAsync(operation);
            }
        }
    }
}