using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;

namespace DataSyncService.Core.Interfaces;

public interface IConflictResolutionService
{
    Task<ApiResponse<SyncConflictDto>> GetConflictAsync(int id);
    Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetConflictsBySessionAsync(string sessionId);
    Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetUnresolvedConflictsAsync();
    Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetConflictsByTableAsync(string tableName);
    Task<ApiResponse<ConflictResolutionResponse>> ResolveConflictAsync(int conflictId, ResolveConflictRequest request);
    Task<ApiResponse<IEnumerable<ConflictResolutionResponse>>> ResolveMultipleConflictsAsync(IEnumerable<int> conflictIds, ResolveConflictRequest request);
    Task<ApiResponse<bool>> EscalateConflictAsync(int conflictId, string reason);
    Task<ApiResponse<PagedResult<SyncConflictDto>>> GetConflictsAsync(PagingRequest pagingRequest, ConflictStatus? status = null);
    Task<ApiResponse<Dictionary<ConflictStatus, int>>> GetConflictStatisticsAsync();
    Task<ApiResponse<IEnumerable<string>>> GetAvailableResolutionStrategiesAsync();
}

public interface IConflictDetectionEngine
{
    Task<IEnumerable<SyncConflictDto>> DetectConflictsAsync(string sessionId);
    Task<IEnumerable<SyncConflictDto>> DetectConflictsForTableAsync(string tableName, IEnumerable<ChangeRecordDto> changes);
    Task<SyncConflictDto?> DetectConflictForRecordAsync(string tableName, string recordId, ChangeRecordDto change);
    Task<bool> HasConflictsAsync(string sessionId);
    Task<bool> HasConflictsForRecordAsync(string tableName, string recordId);
    Task<ConflictResolutionStrategy> GetRecommendedStrategyAsync(SyncConflictDto conflict);
}

public interface IConflictResolver
{
    Task<ConflictResolutionResponse> ResolveConflictAsync(SyncConflictDto conflict, ConflictResolutionStrategy strategy, string? resolvedDataJson = null);
    Task<string> ApplyLastWriteWinsAsync(SyncConflictDto conflict);
    Task<string> ApplyFirstWriteWinsAsync(SyncConflictDto conflict);
    Task<string> ApplyMergeChangesAsync(SyncConflictDto conflict);
    Task<string> ApplyCustomResolutionAsync(SyncConflictDto conflict, string resolvedDataJson);
    Task<bool> ValidateResolutionAsync(SyncConflictDto conflict, string resolvedDataJson);
}