using Microsoft.EntityFrameworkCore;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;

namespace DataSyncService.Infrastructure.Repositories;

public class ChangeRecordRepository : Repository<ChangeRecord>, IChangeRecordRepository
{
    public ChangeRecordRepository(DataSyncDbContext context) : base(context)
    {
    }

    public async Task<ChangeRecord?> GetByChangeIdAsync(string changeId)
    {
        return await _dbSet
            .Include(c => c.RelatedConflicts)
            .FirstOrDefaultAsync(c => c.ChangeId == changeId);
    }

    public async Task<IEnumerable<ChangeRecord>> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet
            .Where(c => c.SessionId == sessionId)
            .OrderBy(c => c.SequenceNumber)
            .ThenBy(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetByTableNameAsync(string tableName)
    {
        return await _dbSet
            .Where(c => c.TableName == tableName)
            .OrderByDescending(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetByRecordIdAsync(string recordId)
    {
        return await _dbSet
            .Where(c => c.RecordId == recordId)
            .OrderByDescending(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetChangesSinceAsync(DateTime timestamp)
    {
        return await _dbSet
            .Where(c => c.ChangeTimestamp > timestamp)
            .OrderBy(c => c.SequenceNumber)
            .ThenBy(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetPendingChangesAsync()
    {
        return await _dbSet
            .Where(c => c.Status == SyncStatus.Pending)
            .OrderBy(c => c.SequenceNumber)
            .ThenBy(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetFailedChangesAsync()
    {
        return await _dbSet
            .Where(c => c.Status == SyncStatus.Failed)
            .OrderByDescending(c => c.LastRetryTime ?? c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetChangesByOperationAsync(ChangeOperation operation)
    {
        return await _dbSet
            .Where(c => c.Operation == operation)
            .OrderByDescending(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChangeRecord>> GetChangesBySiteAsync(string siteId)
    {
        return await _dbSet
            .Where(c => c.SourceSiteId == siteId || c.TargetSiteId == siteId)
            .OrderByDescending(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<long> GetNextSequenceNumberAsync()
    {
        var lastSequence = await _dbSet
            .Where(c => c.SequenceNumber.HasValue)
            .MaxAsync(c => (long?)c.SequenceNumber) ?? 0;
        
        return lastSequence + 1;
    }

    public async Task<IEnumerable<ChangeRecord>> GetChangesForSyncAsync(string tableName, DateTime? lastSyncTime)
    {
        var query = _dbSet.Where(c => c.TableName == tableName);

        if (lastSyncTime.HasValue)
        {
            query = query.Where(c => c.ChangeTimestamp > lastSyncTime.Value);
        }

        return await query
            .OrderBy(c => c.SequenceNumber)
            .ThenBy(c => c.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<Dictionary<ChangeOperation, int>> GetChangeStatisticsAsync()
    {
        return await _dbSet
            .GroupBy(c => c.Operation)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<ChangeRecord>> GetConflictingChangesAsync(string tableName, string recordId)
    {
        return await _dbSet
            .Where(c => c.TableName == tableName && c.RecordId == recordId)
            .Include(c => c.RelatedConflicts)
            .OrderByDescending(c => c.ChangeTimestamp)
            .ToListAsync();
    }
}