using AutoMapper;
using Microsoft.Extensions.Logging;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;

namespace DataSyncService.Core.Services;

public class SyncService : ISyncService
{
    private readonly ISyncSessionRepository _syncSessionRepository;
    private readonly ISyncSiteRepository _syncSiteRepository;
    private readonly ISyncEngine _syncEngine;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public SyncService(
        ISyncSessionRepository syncSessionRepository,
        ISyncSiteRepository syncSiteRepository,
        ISyncEngine syncEngine,
        IMapper mapper,
        ILogger<SyncService> logger,
        IUnitOfWork unitOfWork)
    {
        _syncSessionRepository = syncSessionRepository;
        _syncSiteRepository = syncSiteRepository;
        _syncEngine = syncEngine;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<SyncSessionDto>> StartSyncAsync(CreateSyncSessionRequest request)
    {
        try
        {
            _logger.LogInformation("Starting sync session from {SourceSiteId} to {TargetSiteId}", 
                request.SourceSiteId, request.TargetSiteId);

            // Validate that sites exist
            var sourceSite = await _syncSiteRepository.GetBySiteIdAsync(request.SourceSiteId);
            var targetSite = await _syncSiteRepository.GetBySiteIdAsync(request.TargetSiteId);

            if (sourceSite == null)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = $"Source site '{request.SourceSiteId}' not found",
                    ErrorCode = "SOURCE_SITE_NOT_FOUND"
                };
            }

            if (targetSite == null)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = $"Target site '{request.TargetSiteId}' not found",
                    ErrorCode = "TARGET_SITE_NOT_FOUND"
                };
            }

            // Check if there's already an active sync session
            var hasActiveSession = await _syncSessionRepository.HasActiveSyncSessionAsync(
                request.SourceSiteId, request.TargetSiteId);

            if (hasActiveSession)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "An active sync session already exists between these sites",
                    ErrorCode = "ACTIVE_SESSION_EXISTS"
                };
            }

            // Create new sync session
            var syncSession = _mapper.Map<SyncSession>(request);
            syncSession.Status = SyncStatus.Pending;
            syncSession.StartTime = DateTime.UtcNow;

            await _syncSessionRepository.AddAsync(syncSession);
            await _unitOfWork.SaveChangesAsync();

            // Start the sync engine
            var syncStarted = await _syncEngine.StartSyncAsync(
                syncSession.SessionId, request.SourceSiteId, request.TargetSiteId, request.Mode);

            if (!syncStarted)
            {
                syncSession.Status = SyncStatus.Failed;
                syncSession.ErrorMessage = "Failed to start sync engine";
                await _syncSessionRepository.UpdateAsync(syncSession);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "Failed to start sync engine",
                    ErrorCode = "SYNC_ENGINE_START_FAILED"
                };
            }

            var syncSessionDto = _mapper.Map<SyncSessionDto>(syncSession);

            _logger.LogInformation("Sync session {SessionId} started successfully", syncSession.SessionId);

            return new ApiResponse<SyncSessionDto>
            {
                Success = true,
                Message = "Sync session started successfully",
                Data = syncSessionDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting sync session");
            return new ApiResponse<SyncSessionDto>
            {
                Success = false,
                Message = "An error occurred while starting the sync session",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSessionDto>> StopSyncAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("Stopping sync session {SessionId}", sessionId);

            var syncSession = await _syncSessionRepository.GetBySessionIdAsync(sessionId);
            if (syncSession == null)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "Sync session not found",
                    ErrorCode = "SESSION_NOT_FOUND"
                };
            }

            if (syncSession.Status != SyncStatus.InProgress)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "Sync session is not in progress",
                    ErrorCode = "SESSION_NOT_IN_PROGRESS"
                };
            }

            // Stop the sync engine
            var syncStopped = await _syncEngine.StopSyncAsync(sessionId);

            syncSession.Status = syncStopped ? SyncStatus.Cancelled : SyncStatus.Failed;
            syncSession.EndTime = DateTime.UtcNow;
            syncSession.UpdatedAt = DateTime.UtcNow;

            await _syncSessionRepository.UpdateAsync(syncSession);
            await _unitOfWork.SaveChangesAsync();

            var syncSessionDto = _mapper.Map<SyncSessionDto>(syncSession);

            _logger.LogInformation("Sync session {SessionId} stopped", sessionId);

            return new ApiResponse<SyncSessionDto>
            {
                Success = true,
                Message = "Sync session stopped successfully",
                Data = syncSessionDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping sync session {SessionId}", sessionId);
            return new ApiResponse<SyncSessionDto>
            {
                Success = false,
                Message = "An error occurred while stopping the sync session",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSessionDto>> GetSyncStatusAsync(string sessionId)
    {
        try
        {
            var syncSession = await _syncSessionRepository.GetBySessionIdAsync(sessionId);
            if (syncSession == null)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "Sync session not found",
                    ErrorCode = "SESSION_NOT_FOUND"
                };
            }

            var syncSessionDto = _mapper.Map<SyncSessionDto>(syncSession);

            return new ApiResponse<SyncSessionDto>
            {
                Success = true,
                Message = "Sync status retrieved successfully",
                Data = syncSessionDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync status for session {SessionId}", sessionId);
            return new ApiResponse<SyncSessionDto>
            {
                Success = false,
                Message = "An error occurred while retrieving sync status",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncSessionDto>>> GetActiveSyncSessionsAsync()
    {
        try
        {
            var activeSessions = await _syncSessionRepository.GetActiveSessions();
            var syncSessionDtos = _mapper.Map<IEnumerable<SyncSessionDto>>(activeSessions);

            return new ApiResponse<IEnumerable<SyncSessionDto>>
            {
                Success = true,
                Message = "Active sync sessions retrieved successfully",
                Data = syncSessionDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sync sessions");
            return new ApiResponse<IEnumerable<SyncSessionDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving active sync sessions",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncSessionDto>>> GetSyncHistoryAsync(string? siteId = null, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            IEnumerable<SyncSession> sessions;

            if (!string.IsNullOrEmpty(siteId))
            {
                sessions = await _syncSessionRepository.GetSessionsBySiteAsync(siteId);
            }
            else
            {
                sessions = await _syncSessionRepository.GetAllAsync();
            }

            var pagedSessions = sessions
                .OrderByDescending(s => s.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var syncSessionDtos = _mapper.Map<IEnumerable<SyncSessionDto>>(pagedSessions);

            return new ApiResponse<IEnumerable<SyncSessionDto>>
            {
                Success = true,
                Message = "Sync history retrieved successfully",
                Data = syncSessionDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync history");
            return new ApiResponse<IEnumerable<SyncSessionDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving sync history",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncSessionDto>> SyncTableAsync(SyncDataRequest request)
    {
        try
        {
            _logger.LogInformation("Starting table sync for {TableName} from {SourceSiteId} to {TargetSiteId}", 
                request.TableName, request.SourceSiteId, request.TargetSiteId);

            // Create a sync session for this table sync
            var createRequest = new CreateSyncSessionRequest
            {
                SourceSiteId = request.SourceSiteId,
                TargetSiteId = request.TargetSiteId,
                Direction = SyncDirection.Outgoing,
                Mode = request.Mode,
                Priority = SyncPriority.Normal
            };

            var sessionResponse = await StartSyncAsync(createRequest);
            if (!sessionResponse.Success)
            {
                return sessionResponse;
            }

            var sessionId = sessionResponse.Data!.SessionId;

            // Perform table sync
            var syncResult = await _syncEngine.SyncTableAsync(sessionId, request.TableName, request.LastSyncTime);

            if (!syncResult)
            {
                return new ApiResponse<SyncSessionDto>
                {
                    Success = false,
                    Message = "Table sync failed",
                    ErrorCode = "TABLE_SYNC_FAILED"
                };
            }

            // Get updated session status
            return await GetSyncStatusAsync(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing table {TableName}", request.TableName);
            return new ApiResponse<SyncSessionDto>
            {
                Success = false,
                Message = "An error occurred while syncing the table",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<SyncStatusSummary>> GetSyncStatusSummaryAsync()
    {
        try
        {
            var sessionStatistics = await _syncSessionRepository.GetSessionStatisticsAsync();
            var siteStatistics = await _syncSiteRepository.GetSiteStatisticsAsync();

            var summary = new SyncStatusSummary
            {
                ActiveSessions = sessionStatistics.GetValueOrDefault(SyncStatus.InProgress, 0),
                PendingSessions = sessionStatistics.GetValueOrDefault(SyncStatus.Pending, 0),
                FailedSessions = sessionStatistics.GetValueOrDefault(SyncStatus.Failed, 0),
                TotalSites = siteStatistics.Values.Sum(),
                ActiveSites = siteStatistics.GetValueOrDefault(SiteStatus.Active, 0),
                SyncStatistics = sessionStatistics.ToDictionary(
                    kvp => kvp.Key.ToString(), 
                    kvp => kvp.Value)
            };

            // Get last sync time
            var lastSession = await _syncSessionRepository.GetAllAsync();
            var lastCompletedSession = lastSession
                .Where(s => s.Status == SyncStatus.Completed)
                .OrderByDescending(s => s.EndTime)
                .FirstOrDefault();

            if (lastCompletedSession != null)
            {
                summary.LastSyncTime = lastCompletedSession.EndTime ?? lastCompletedSession.StartTime;
            }

            return new ApiResponse<SyncStatusSummary>
            {
                Success = true,
                Message = "Sync status summary retrieved successfully",
                Data = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync status summary");
            return new ApiResponse<SyncStatusSummary>
            {
                Success = false,
                Message = "An error occurred while retrieving sync status summary",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<bool>> CancelSyncAsync(string sessionId)
    {
        var result = await StopSyncAsync(sessionId);
        return new ApiResponse<bool>
        {
            Success = result.Success,
            Message = result.Message,
            Data = result.Success,
            ErrorCode = result.ErrorCode
        };
    }

    public async Task<ApiResponse<bool>> RetrySyncAsync(string sessionId)
    {
        try
        {
            var syncSession = await _syncSessionRepository.GetBySessionIdAsync(sessionId);
            if (syncSession == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Sync session not found",
                    ErrorCode = "SESSION_NOT_FOUND"
                };
            }

            if (syncSession.Status != SyncStatus.Failed)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only failed sync sessions can be retried",
                    ErrorCode = "INVALID_SESSION_STATUS"
                };
            }

            // Reset session status
            syncSession.Status = SyncStatus.Pending;
            syncSession.ErrorMessage = null;
            syncSession.UpdatedAt = DateTime.UtcNow;

            await _syncSessionRepository.UpdateAsync(syncSession);
            await _unitOfWork.SaveChangesAsync();

            // Restart the sync engine
            var syncStarted = await _syncEngine.StartSyncAsync(
                syncSession.SessionId, syncSession.SourceSiteId, syncSession.TargetSiteId, syncSession.Mode);

            return new ApiResponse<bool>
            {
                Success = syncStarted,
                Message = syncStarted ? "Sync session restarted successfully" : "Failed to restart sync session",
                Data = syncStarted,
                ErrorCode = syncStarted ? null : "SYNC_ENGINE_START_FAILED"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying sync session {SessionId}", sessionId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while retrying the sync session",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<SyncSessionDto>>> GetSyncSessionsAsync(PagingRequest pagingRequest, SyncStatus? status = null)
    {
        try
        {
            var (sessions, totalCount) = await _syncSessionRepository.GetPagedAsync(
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                status.HasValue ? s => s.Status == status.Value : null,
                s => s.StartTime,
                pagingRequest.SortDescending);

            var syncSessionDtos = _mapper.Map<IEnumerable<SyncSessionDto>>(sessions);

            var pagedResult = new PagedResult<SyncSessionDto>
            {
                Items = syncSessionDtos,
                TotalCount = totalCount,
                PageNumber = pagingRequest.PageNumber,
                PageSize = pagingRequest.PageSize
            };

            return new ApiResponse<PagedResult<SyncSessionDto>>
            {
                Success = true,
                Message = "Sync sessions retrieved successfully",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sync sessions");
            return new ApiResponse<PagedResult<SyncSessionDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving sync sessions",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }
}