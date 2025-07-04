using Microsoft.EntityFrameworkCore;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;

namespace DataSyncService.Infrastructure.Repositories;

public class SyncSessionRepository : Repository<SyncSession>, ISyncSessionRepository
{
    public SyncSessionRepository(DataSyncDbContext context) : base(context)
    {
    }

    public async Task<SyncSession?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet
            .Include(s => s.SyncLogs)
            .Include(s => s.ChangeRecords)
            .Include(s => s.SyncConflicts)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<IEnumerable<SyncSession>> GetActiveSessions()
    {
        return await _dbSet
            .Where(s => s.Status == SyncStatus.InProgress || s.Status == SyncStatus.Pending)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncSession>> GetSessionsBySiteAsync(string siteId)
    {
        return await _dbSet
            .Where(s => s.SourceSiteId == siteId || s.TargetSiteId == siteId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncSession>> GetSessionsByStatusAsync(SyncStatus status)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncSession>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<SyncSession?> GetLatestSessionBySiteAsync(string sourceSiteId, string targetSiteId)
    {
        return await _dbSet
            .Where(s => s.SourceSiteId == sourceSiteId && s.TargetSiteId == targetSiteId)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<SyncStatus, int>> GetSessionStatisticsAsync()
    {
        return await _dbSet
            .GroupBy(s => s.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<SyncSession>> GetFailedSessionsAsync()
    {
        return await _dbSet
            .Where(s => s.Status == SyncStatus.Failed)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasActiveSyncSessionAsync(string sourceSiteId, string targetSiteId)
    {
        return await _dbSet
            .AnyAsync(s => s.SourceSiteId == sourceSiteId && 
                          s.TargetSiteId == targetSiteId && 
                          (s.Status == SyncStatus.InProgress || s.Status == SyncStatus.Pending));
    }
}