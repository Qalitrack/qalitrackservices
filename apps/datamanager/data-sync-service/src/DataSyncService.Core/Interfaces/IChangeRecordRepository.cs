using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface IChangeRecordRepository : IRepository<ChangeRecord>
{
    Task<ChangeRecord?> GetByChangeIdAsync(string changeId);
    Task<IEnumerable<ChangeRecord>> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<ChangeRecord>> GetByTableNameAsync(string tableName);
    Task<IEnumerable<ChangeRecord>> GetByRecordIdAsync(string recordId);
    Task<IEnumerable<ChangeRecord>> GetChangesSinceAsync(DateTime timestamp);
    Task<IEnumerable<ChangeRecord>> GetPendingChangesAsync();
    Task<IEnumerable<ChangeRecord>> GetFailedChangesAsync();
    Task<IEnumerable<ChangeRecord>> GetChangesByOperationAsync(ChangeOperation operation);
    Task<IEnumerable<ChangeRecord>> GetChangesBySiteAsync(string siteId);
    Task<long> GetNextSequenceNumberAsync();
    Task<IEnumerable<ChangeRecord>> GetChangesForSyncAsync(string tableName, DateTime? lastSyncTime);
    Task<Dictionary<ChangeOperation, int>> GetChangeStatisticsAsync();
    Task<IEnumerable<ChangeRecord>> GetConflictingChangesAsync(string tableName, string recordId);
}