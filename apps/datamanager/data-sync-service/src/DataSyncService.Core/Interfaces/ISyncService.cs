using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface ISyncService
{
    Task<ApiResponse<SyncSessionDto>> StartSyncAsync(CreateSyncSessionRequest request);
    Task<ApiResponse<SyncSessionDto>> StopSyncAsync(string sessionId);
    Task<ApiResponse<SyncSessionDto>> GetSyncStatusAsync(string sessionId);
    Task<ApiResponse<IEnumerable<SyncSessionDto>>> GetActiveSyncSessionsAsync();
    Task<ApiResponse<IEnumerable<SyncSessionDto>>> GetSyncHistoryAsync(string? siteId = null, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<SyncSessionDto>> SyncTableAsync(SyncDataRequest request);
    Task<ApiResponse<SyncStatusSummary>> GetSyncStatusSummaryAsync();
    Task<ApiResponse<bool>> CancelSyncAsync(string sessionId);
    Task<ApiResponse<bool>> RetrySyncAsync(string sessionId);
    Task<ApiResponse<PagedResult<SyncSessionDto>>> GetSyncSessionsAsync(PagingRequest pagingRequest, SyncStatus? status = null);
}

public interface ISyncEngine
{
    Task<bool> StartSyncAsync(string sessionId, string sourceSiteId, string targetSiteId, SyncMode mode);
    Task<bool> StopSyncAsync(string sessionId);
    Task<bool> SyncTableAsync(string sessionId, string tableName, DateTime? lastSyncTime = null);
    Task<bool> ProcessChangeRecordsAsync(string sessionId, IEnumerable<ChangeRecordDto> changes);
    Task<bool> DetectConflictsAsync(string sessionId);
    Task<bool> ValidateDataIntegrityAsync(string sessionId);
    Task<SyncSessionDto> GetSyncProgressAsync(string sessionId);
    Task<bool> PerformFullSyncAsync(string sessionId, string tableName);
    Task<bool> PerformIncrementalSyncAsync(string sessionId, string tableName, DateTime lastSyncTime);
    Task<bool> PerformDeltaSyncAsync(string sessionId, string tableName, DateTime lastSyncTime);
}