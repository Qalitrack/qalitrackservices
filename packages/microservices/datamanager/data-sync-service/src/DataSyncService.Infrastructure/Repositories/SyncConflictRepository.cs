using Microsoft.EntityFrameworkCore;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;

namespace DataSyncService.Infrastructure.Repositories;

public class SyncConflictRepository : Repository<SyncConflict>, ISyncConflictRepository
{
    public SyncConflictRepository(DataSyncDbContext context) : base(context)
    {
    }

    public async Task<SyncConflict?> GetByConflictIdAsync(string conflictId)
    {
        return await _dbSet
            .Include(c => c.RelatedChanges)
            .FirstOrDefaultAsync(c => c.ConflictId == conflictId);
    }

    public async Task<IEnumerable<SyncConflict>> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetByStatusAsync(ConflictStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetByTableNameAsync(string tableName)
    {
        return await _dbSet
            .Where(c => c.TableName == tableName)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetByRecordIdAsync(string recordId)
    {
        return await _dbSet
            .Where(c => c.RecordId == recordId)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetUnresolvedConflictsAsync()
    {
        return await _dbSet
            .Where(c => c.Status == ConflictStatus.Detected || c.Status == ConflictStatus.InResolution)
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetConflictsByPriorityAsync()
    {
        return await _dbSet
            .Where(c => c.Status != ConflictStatus.Resolved)
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetConflictsBySiteAsync(string siteId)
    {
        return await _dbSet
            .Where(c => c.SourceSiteId == siteId || c.TargetSiteId == siteId)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncConflict>> GetConflictsByResolutionStrategyAsync(ConflictResolutionStrategy strategy)
    {
        return await _dbSet
            .Where(c => c.ResolutionStrategy == strategy)
            .OrderByDescending(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<ConflictStatus, int>> GetConflictStatisticsAsync()
    {
        return await _dbSet
            .GroupBy(c => c.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<SyncConflict>> GetOldUnresolvedConflictsAsync(DateTime cutoffDate)
    {
        return await _dbSet
            .Where(c => (c.Status == ConflictStatus.Detected || c.Status == ConflictStatus.InResolution) && 
                       c.ConflictDetectedAt < cutoffDate)
            .OrderBy(c => c.ConflictDetectedAt)
            .ToListAsync();
    }

    public async Task<bool> HasUnresolvedConflictsForRecordAsync(string tableName, string recordId)
    {
        return await _dbSet
            .AnyAsync(c => c.TableName == tableName && 
                          c.RecordId == recordId && 
                          (c.Status == ConflictStatus.Detected || c.Status == ConflictStatus.InResolution));
    }
}