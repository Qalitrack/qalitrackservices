using AutoMapper;
using Microsoft.Extensions.Logging;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;

namespace DataSyncService.Core.Services;

public class ConflictResolutionService : IConflictResolutionService
{
    private readonly ISyncConflictRepository _syncConflictRepository;
    private readonly IConflictResolver _conflictResolver;
    private readonly IMapper _mapper;
    private readonly ILogger<ConflictResolutionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ConflictResolutionService(
        ISyncConflictRepository syncConflictRepository,
        IConflictResolver conflictResolver,
        IMapper mapper,
        ILogger<ConflictResolutionService> logger,
        IUnitOfWork unitOfWork)
    {
        _syncConflictRepository = syncConflictRepository;
        _conflictResolver = conflictResolver;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<SyncConflictDto>> GetConflictAsync(int id)
    {
        try
        {
            var conflict = await _syncConflictRepository.GetByIdAsync(id);
            if (conflict == null)
            {
                return new ApiResponse<SyncConflictDto>
                {
                    Success = false,
                    Message = "Conflict not found",
                    ErrorCode = "CONFLICT_NOT_FOUND"
                };
            }

            var conflictDto = _mapper.Map<SyncConflictDto>(conflict);

            return new ApiResponse<SyncConflictDto>
            {
                Success = true,
                Message = "Conflict retrieved successfully",
                Data = conflictDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conflict {ConflictId}", id);
            return new ApiResponse<SyncConflictDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the conflict",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetConflictsBySessionAsync(string sessionId)
    {
        try
        {
            var conflicts = await _syncConflictRepository.GetBySessionIdAsync(sessionId);
            var conflictDtos = _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);

            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = true,
                Message = "Conflicts retrieved successfully",
                Data = conflictDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conflicts for session {SessionId}", sessionId);
            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving conflicts",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetUnresolvedConflictsAsync()
    {
        try
        {
            var conflicts = await _syncConflictRepository.GetUnresolvedConflictsAsync();
            var conflictDtos = _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);

            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = true,
                Message = "Unresolved conflicts retrieved successfully",
                Data = conflictDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unresolved conflicts");
            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving unresolved conflicts",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<SyncConflictDto>>> GetConflictsByTableAsync(string tableName)
    {
        try
        {
            var conflicts = await _syncConflictRepository.GetByTableNameAsync(tableName);
            var conflictDtos = _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);

            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = true,
                Message = $"Conflicts for table {tableName} retrieved successfully",
                Data = conflictDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conflicts for table {TableName}", tableName);
            return new ApiResponse<IEnumerable<SyncConflictDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving conflicts for the table",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<ConflictResolutionResponse>> ResolveConflictAsync(int conflictId, ResolveConflictRequest request)
    {
        try
        {
            _logger.LogInformation("Resolving conflict {ConflictId} with strategy {Strategy}", 
                conflictId, request.ResolutionStrategy);

            var conflict = await _syncConflictRepository.GetByIdAsync(conflictId);
            if (conflict == null)
            {
                return new ApiResponse<ConflictResolutionResponse>
                {
                    Success = false,
                    Message = "Conflict not found",
                    ErrorCode = "CONFLICT_NOT_FOUND"
                };
            }

            if (conflict.Status == ConflictStatus.Resolved)
            {
                return new ApiResponse<ConflictResolutionResponse>
                {
                    Success = false,
                    Message = "Conflict is already resolved",
                    ErrorCode = "CONFLICT_ALREADY_RESOLVED"
                };
            }

            var conflictDto = _mapper.Map<SyncConflictDto>(conflict);
            var resolutionResponse = await _conflictResolver.ResolveConflictAsync(
                conflictDto, request.ResolutionStrategy, request.ResolvedDataJson);

            if (resolutionResponse.Success)
            {
                // Update the conflict entity
                _mapper.Map(request, conflict);
                conflict.Status = ConflictStatus.Resolved;
                conflict.ResolvedAt = DateTime.UtcNow;
                conflict.ResolvedDataJson = resolutionResponse.ResolvedConflict?.ResolvedDataJson;

                await _syncConflictRepository.UpdateAsync(conflict);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Conflict {ConflictId} resolved successfully", conflictId);
            }

            return new ApiResponse<ConflictResolutionResponse>
            {
                Success = resolutionResponse.Success,
                Message = resolutionResponse.Message,
                Data = resolutionResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving conflict {ConflictId}", conflictId);
            return new ApiResponse<ConflictResolutionResponse>
            {
                Success = false,
                Message = "An error occurred while resolving the conflict",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ConflictResolutionResponse>>> ResolveMultipleConflictsAsync(
        IEnumerable<int> conflictIds, ResolveConflictRequest request)
    {
        try
        {
            _logger.LogInformation("Resolving {ConflictCount} conflicts with strategy {Strategy}", 
                conflictIds.Count(), request.ResolutionStrategy);

            var responses = new List<ConflictResolutionResponse>();

            foreach (var conflictId in conflictIds)
            {
                var response = await ResolveConflictAsync(conflictId, request);
                if (response.Data != null)
                {
                    responses.Add(response.Data);
                }
            }

            return new ApiResponse<IEnumerable<ConflictResolutionResponse>>
            {
                Success = true,
                Message = $"Processed {responses.Count} conflicts",
                Data = responses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving multiple conflicts");
            return new ApiResponse<IEnumerable<ConflictResolutionResponse>>
            {
                Success = false,
                Message = "An error occurred while resolving conflicts",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<bool>> EscalateConflictAsync(int conflictId, string reason)
    {
        try
        {
            _logger.LogInformation("Escalating conflict {ConflictId}", conflictId);

            var conflict = await _syncConflictRepository.GetByIdAsync(conflictId);
            if (conflict == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Conflict not found",
                    ErrorCode = "CONFLICT_NOT_FOUND"
                };
            }

            conflict.Status = ConflictStatus.Escalated;
            conflict.ResolutionReason = reason;
            conflict.UpdatedAt = DateTime.UtcNow;

            await _syncConflictRepository.UpdateAsync(conflict);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Conflict {ConflictId} escalated successfully", conflictId);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Conflict escalated successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating conflict {ConflictId}", conflictId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while escalating the conflict",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<SyncConflictDto>>> GetConflictsAsync(
        PagingRequest pagingRequest, ConflictStatus? status = null)
    {
        try
        {
            var (conflicts, totalCount) = await _syncConflictRepository.GetPagedAsync(
                pagingRequest.PageNumber,
                pagingRequest.PageSize,
                status.HasValue ? c => c.Status == status.Value : null,
                c => c.ConflictDetectedAt,
                pagingRequest.SortDescending);

            var conflictDtos = _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);

            var pagedResult = new PagedResult<SyncConflictDto>
            {
                Items = conflictDtos,
                TotalCount = totalCount,
                PageNumber = pagingRequest.PageNumber,
                PageSize = pagingRequest.PageSize
            };

            return new ApiResponse<PagedResult<SyncConflictDto>>
            {
                Success = true,
                Message = "Conflicts retrieved successfully",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged conflicts");
            return new ApiResponse<PagedResult<SyncConflictDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving conflicts",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<Dictionary<ConflictStatus, int>>> GetConflictStatisticsAsync()
    {
        try
        {
            var statistics = await _syncConflictRepository.GetConflictStatisticsAsync();

            return new ApiResponse<Dictionary<ConflictStatus, int>>
            {
                Success = true,
                Message = "Conflict statistics retrieved successfully",
                Data = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conflict statistics");
            return new ApiResponse<Dictionary<ConflictStatus, int>>
            {
                Success = false,
                Message = "An error occurred while retrieving conflict statistics",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> GetAvailableResolutionStrategiesAsync()
    {
        try
        {
            var strategies = Enum.GetNames<ConflictResolutionStrategy>().ToList();

            return new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Message = "Resolution strategies retrieved successfully",
                Data = strategies
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resolution strategies");
            return new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "An error occurred while retrieving resolution strategies",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }
}