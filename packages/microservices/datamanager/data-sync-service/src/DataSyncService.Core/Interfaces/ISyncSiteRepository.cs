using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface ISyncSiteRepository : IRepository<SyncSite>
{
    Task<SyncSite?> GetBySiteIdAsync(string siteId);
    Task<IEnumerable<SyncSite>> GetActiveSites();
    Task<IEnumerable<SyncSite>> GetSitesByStatusAsync(SiteStatus status);
    Task<SyncSite?> GetMainSiteAsync();
    Task<IEnumerable<SyncSite>> GetSitesByPriorityAsync();
    Task<bool> SiteExistsAsync(string siteId);
    Task<IEnumerable<SyncSite>> GetSitesRequiringHealthCheckAsync();
    Task UpdateSiteStatusAsync(string siteId, SiteStatus status);
    Task UpdateLastSyncTimeAsync(string siteId, DateTime lastSyncTime);
    Task<Dictionary<SiteStatus, int>> GetSiteStatisticsAsync();
}