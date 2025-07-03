using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface ISyncSessionRepository : IRepository<SyncSession>
{
    Task<SyncSession?> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<SyncSession>> GetActiveSessions();
    Task<IEnumerable<SyncSession>> GetSessionsBySiteAsync(string siteId);
    Task<IEnumerable<SyncSession>> GetSessionsByStatusAsync(SyncStatus status);
    Task<IEnumerable<SyncSession>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SyncSession?> GetLatestSessionBySiteAsync(string sourceSiteId, string targetSiteId);
    Task<Dictionary<SyncStatus, int>> GetSessionStatisticsAsync();
    Task<IEnumerable<SyncSession>> GetFailedSessionsAsync();
    Task<bool> HasActiveSyncSessionAsync(string sourceSiteId, string targetSiteId);
}