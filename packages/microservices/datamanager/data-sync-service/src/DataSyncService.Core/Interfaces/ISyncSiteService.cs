using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface ISyncSiteService
{
    Task<ApiResponse<SyncSiteDto>> CreateSiteAsync(CreateSyncSiteRequest request);
    Task<ApiResponse<SyncSiteDto>> UpdateSiteAsync(int id, UpdateSyncSiteRequest request);
    Task<ApiResponse<bool>> DeleteSiteAsync(int id);
    Task<ApiResponse<SyncSiteDto>> GetSiteAsync(int id);
    Task<ApiResponse<SyncSiteDto>> GetSiteBySiteIdAsync(string siteId);
    Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetAllSitesAsync();
    Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetActiveSitesAsync();
    Task<ApiResponse<IEnumerable<SyncSiteDto>>> GetSitesByStatusAsync(SiteStatus status);
    Task<ApiResponse<HealthCheckDto>> CheckSiteHealthAsync(string siteId);
    Task<ApiResponse<IEnumerable<HealthCheckDto>>> CheckAllSitesHealthAsync();
    Task<ApiResponse<bool>> UpdateSiteStatusAsync(string siteId, SiteStatus status);
    Task<ApiResponse<PagedResult<SyncSiteDto>>> GetSitesAsync(PagingRequest pagingRequest, SiteStatus? status = null);
    Task<ApiResponse<Dictionary<SiteStatus, int>>> GetSiteStatisticsAsync();
}

public interface ISiteHealthMonitor
{
    Task<HealthCheckDto> CheckSiteHealthAsync(string siteId);
    Task<IEnumerable<HealthCheckDto>> CheckAllSitesHealthAsync();
    Task<bool> IsSiteHealthyAsync(string siteId);
    Task StartMonitoringAsync();
    Task StopMonitoringAsync();
    Task<bool> PingSiteAsync(string siteId);
    Task<bool> CheckDatabaseConnectionAsync(string siteId);
    Task<bool> CheckApiEndpointAsync(string siteId);
}