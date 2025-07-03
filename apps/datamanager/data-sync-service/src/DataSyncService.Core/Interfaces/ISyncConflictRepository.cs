using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface ISyncConflictRepository : IRepository<SyncConflict>
{
    Task<SyncConflict?> GetByConflictIdAsync(string conflictId);
    Task<IEnumerable<SyncConflict>> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<SyncConflict>> GetByStatusAsync(ConflictStatus status);
    Task<IEnumerable<SyncConflict>> GetByTableNameAsync(string tableName);
    Task<IEnumerable<SyncConflict>> GetByRecordIdAsync(string recordId);
    Task<IEnumerable<SyncConflict>> GetUnresolvedConflictsAsync();
    Task<IEnumerable<SyncConflict>> GetConflictsByPriorityAsync();
    Task<IEnumerable<SyncConflict>> GetConflictsBySiteAsync(string siteId);
    Task<IEnumerable<SyncConflict>> GetConflictsByResolutionStrategyAsync(ConflictResolutionStrategy strategy);
    Task<Dictionary<ConflictStatus, int>> GetConflictStatisticsAsync();
    Task<IEnumerable<SyncConflict>> GetOldUnresolvedConflictsAsync(DateTime cutoffDate);
    Task<bool> HasUnresolvedConflictsForRecordAsync(string tableName, string recordId);
}