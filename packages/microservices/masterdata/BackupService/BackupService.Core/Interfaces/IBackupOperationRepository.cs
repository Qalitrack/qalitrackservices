using BackupService.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackupService.Core.Interfaces;

public interface IBackupOperationRepository
{
    Task AddAsync(BackupOperationRecord operationRecord);
    Task<BackupOperationRecord?> GetByCommandIdAsync(string commandId);
    Task<List<BackupOperationRecord>> GetRecentAsync(int count);
    Task UpdateAsync(BackupOperationRecord operationRecord);
}