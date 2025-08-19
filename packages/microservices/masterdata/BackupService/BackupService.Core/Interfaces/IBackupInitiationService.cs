using BackupService.Core.Entities;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.BackupEvents;

namespace BackupService.Core.Interfaces;

public interface IBackupInitiationService
{
    Task<string> InitiateBackupCommandAsync(BackupCommandType commandType, bool isCritical = false, string? chainId = null);
    Task<BackupOrchestrationResult?> GetCommandResultAsync(string commandId, TimeSpan? timeout = null);
    Task<List<BackupOperationRecord>> GetRecentBackupOperationsAsync(int count = 10);
    
}