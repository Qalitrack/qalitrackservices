using Microsoft.EntityFrameworkCore;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;

namespace DataSyncService.Infrastructure.Repositories;

public class SyncSiteRepository : Repository<SyncSite>, ISyncSiteRepository
{
    public SyncSiteRepository(DataSyncDbContext context) : base(context)
    {
    }

    public async Task<SyncSite?> GetBySiteIdAsync(string siteId)
    {
        return await _dbSet
            .Include(s => s.HealthChecks.OrderByDescending(h => h.CheckTime).Take(5))
            .FirstOrDefaultAsync(s => s.SiteId == siteId);
    }

    public async Task<IEnumerable<SyncSite>> GetActiveSites()
    {
        return await _dbSet
            .Where(s => s.Status == SiteStatus.Active)
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<SyncSite>> GetSitesByStatusAsync(SiteStatus status)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<SyncSite?> GetMainSiteAsync()
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.IsMain);
    }

    public async Task<IEnumerable<SyncSite>> GetSitesByPriorityAsync()
    {
        return await _dbSet
            .Where(s => s.Status == SiteStatus.Active)
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> SiteExistsAsync(string siteId)
    {
        return await _dbSet.AnyAsync(s => s.SiteId == siteId);
    }

    public async Task<IEnumerable<SyncSite>> GetSitesRequiringHealthCheckAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5); // Sites not checked in last 5 minutes
        return await _dbSet
            .Where(s => s.Status == SiteStatus.Active && s.LastHealthCheck < cutoffTime)
            .OrderBy(s => s.LastHealthCheck)
            .ToListAsync();
    }

    public async Task UpdateSiteStatusAsync(string siteId, SiteStatus status)
    {
        var site = await _dbSet.FirstOrDefaultAsync(s => s.SiteId == siteId);
        if (site != null)
        {
            site.Status = status;
            site.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(site);
        }
    }

    public async Task UpdateLastSyncTimeAsync(string siteId, DateTime lastSyncTime)
    {
        var site = await _dbSet.FirstOrDefaultAsync(s => s.SiteId == siteId);
        if (site != null)
        {
            site.LastSyncTime = lastSyncTime;
            site.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(site);
        }
    }

    public async Task<Dictionary<SiteStatus, int>> GetSiteStatisticsAsync()
    {
        return await _dbSet
            .GroupBy(s => s.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }
}