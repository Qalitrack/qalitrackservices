using AutoMapper;
using Microsoft.Extensions.Logging;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;

namespace DataSyncService.Infrastructure.Services;

public class ConflictDetectionEngine : IConflictDetectionEngine
{
    private readonly IChangeRecordRepository _changeRecordRepository;
    private readonly ISyncConflictRepository _syncConflictRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConflictDetectionEngine> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ConflictDetectionEngine(
        IChangeRecordRepository changeRecordRepository,
        ISyncConflictRepository syncConflictRepository,
        IMapper mapper,
        ILogger<ConflictDetectionEngine> logger,
        IUnitOfWork unitOfWork)
    {
        _changeRecordRepository = changeRecordRepository;
        _syncConflictRepository = syncConflictRepository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SyncConflictDto>> DetectConflictsAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("Detecting conflicts for session {SessionId}", sessionId);

            var changes = await _changeRecordRepository.GetBySessionIdAsync(sessionId);
            var conflicts = new List<SyncConflict>();

            // Group changes by table and record
            var changeGroups = changes
                .GroupBy(c => new { c.TableName, c.RecordId })
                .Where(g => g.Count() > 1);

            foreach (var group in changeGroups)
            {
                var conflictingChanges = group.OrderBy(c => c.ChangeTimestamp).ToList();
                
                // Check for conflicts between changes
                for (int i = 1; i < conflictingChanges.Count; i++)
                {
                    var earlier = conflictingChanges[i - 1];
                    var later = conflictingChanges[i];

                    var conflict = await AnalyzeConflictAsync(earlier, later);
                    if (conflict != null)
                    {
                        conflicts.Add(conflict);
                    }
                }
            }

            // Save detected conflicts
            foreach (var conflict in conflicts)
            {
                await _syncConflictRepository.AddAsync(conflict);
            }

            await _unitOfWork.SaveChangesAsync();

            var conflictDtos = _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);

            _logger.LogInformation("Detected {ConflictCount} conflicts for session {SessionId}", 
                conflicts.Count, sessionId);

            return conflictDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting conflicts for session {SessionId}", sessionId);
            return new List<SyncConflictDto>();
        }
    }

    public async Task<IEnumerable<SyncConflictDto>> DetectConflictsForTableAsync(string tableName, IEnumerable<ChangeRecordDto> changes)
    {
        try
        {
            _logger.LogInformation("Detecting conflicts for table {TableName}", tableName);

            var conflicts = new List<SyncConflict>();
            var changesList = changes.ToList();

            // Group by record ID
            var recordGroups = changesList.GroupBy(c => c.RecordId);

            foreach (var group in recordGroups)
            {
                var recordChanges = group.OrderBy(c => c.ChangeTimestamp).ToList();
                
                if (recordChanges.Count > 1)
                {
                    // Check for conflicts
                    for (int i = 1; i < recordChanges.Count; i++)
                    {
                        var conflict = await DetectConflictBetweenChangesAsync(recordChanges[i - 1], recordChanges[i]);
                        if (conflict != null)
                        {
                            var conflictEntity = _mapper.Map<SyncConflict>(conflict);
                            conflicts.Add(conflictEntity);
                        }
                    }
                }
            }

            return _mapper.Map<IEnumerable<SyncConflictDto>>(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting conflicts for table {TableName}", tableName);
            return new List<SyncConflictDto>();
        }
    }

    public async Task<SyncConflictDto?> DetectConflictForRecordAsync(string tableName, string recordId, ChangeRecordDto change)
    {
        try
        {
            // Get existing changes for this record
            var existingChanges = await _changeRecordRepository.GetConflictingChangesAsync(tableName, recordId);
            
            foreach (var existingChange in existingChanges)
            {
                var existingChangeDto = _mapper.Map<ChangeRecordDto>(existingChange);
                var conflict = await DetectConflictBetweenChangesAsync(existingChangeDto, change);
                
                if (conflict != null)
                {
                    return conflict;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting conflict for record {RecordId} in table {TableName}", 
                recordId, tableName);
            return null;
        }
    }

    public async Task<bool> HasConflictsAsync(string sessionId)
    {
        try
        {
            var conflicts = await _syncConflictRepository.GetBySessionIdAsync(sessionId);
            return conflicts.Any(c => c.Status != ConflictStatus.Resolved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicts in session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> HasConflictsForRecordAsync(string tableName, string recordId)
    {
        try
        {
            return await _syncConflictRepository.HasUnresolvedConflictsForRecordAsync(tableName, recordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicts for record {RecordId} in table {TableName}", 
                recordId, tableName);
            return false;
        }
    }

    public async Task<ConflictResolutionStrategy> GetRecommendedStrategyAsync(SyncConflictDto conflict)
    {
        try
        {
            // Implement logic to recommend resolution strategy based on:
            // - Conflict type
            // - Data importance
            // - Site priority
            // - Historical patterns

            return conflict.ConflictType switch
            {
                "DataMismatch" => ConflictResolutionStrategy.LastWriteWins,
                "DuplicateRecord" => ConflictResolutionStrategy.MergeChanges,
                "DeleteConflict" => ConflictResolutionStrategy.Manual,
                _ => ConflictResolutionStrategy.LastWriteWins
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended strategy for conflict {ConflictId}", 
                conflict.ConflictId);
            return ConflictResolutionStrategy.Manual;
        }
    }

    private async Task<SyncConflict?> AnalyzeConflictAsync(ChangeRecord earlier, ChangeRecord later)
    {
        try
        {
            // Different conflict scenarios
            if (earlier.Operation == ChangeOperation.Delete && later.Operation == ChangeOperation.Update)
            {
                return CreateConflict(earlier, later, "DeleteUpdateConflict", 
                    "Record was deleted in one site but updated in another");
            }

            if (earlier.Operation == ChangeOperation.Update && later.Operation == ChangeOperation.Delete)
            {
                return CreateConflict(earlier, later, "UpdateDeleteConflict", 
                    "Record was updated in one site but deleted in another");
            }

            if (earlier.Operation == ChangeOperation.Update && later.Operation == ChangeOperation.Update)
            {
                // Check if data is different
                if (!string.Equals(earlier.NewDataJson, later.NewDataJson, StringComparison.OrdinalIgnoreCase))
                {
                    return CreateConflict(earlier, later, "DataMismatch", 
                        "Same record updated differently in multiple sites");
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing conflict between changes {ChangeId1} and {ChangeId2}", 
                earlier.ChangeId, later.ChangeId);
            return null;
        }
    }

    private async Task<SyncConflictDto?> DetectConflictBetweenChangesAsync(ChangeRecordDto change1, ChangeRecordDto change2)
    {
        try
        {
            // Check for various conflict scenarios
            if (change1.RecordId != change2.RecordId || change1.TableName != change2.TableName)
                return null;

            var conflictType = DetermineConflictType(change1, change2);
            if (conflictType == null)
                return null;

            return new SyncConflictDto
            {
                ConflictId = Guid.NewGuid().ToString(),
                SessionId = change1.SessionId,
                TableName = change1.TableName,
                RecordId = change1.RecordId,
                Status = ConflictStatus.Detected,
                ConflictType = conflictType,
                Description = GetConflictDescription(conflictType),
                SourceDataJson = change1.NewDataJson ?? change1.OldDataJson ?? "",
                TargetDataJson = change2.NewDataJson ?? change2.OldDataJson ?? "",
                SourceSiteId = change1.SourceSiteId,
                TargetSiteId = change2.TargetSiteId ?? change2.SourceSiteId,
                ConflictDetectedAt = DateTime.UtcNow,
                Priority = GetConflictPriority(conflictType)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting conflict between changes");
            return null;
        }
    }

    private SyncConflict CreateConflict(ChangeRecord earlier, ChangeRecord later, string conflictType, string description)
    {
        return new SyncConflict
        {
            ConflictId = Guid.NewGuid().ToString(),
            SessionId = later.SessionId,
            TableName = later.TableName,
            RecordId = later.RecordId,
            Status = ConflictStatus.Detected,
            ConflictType = conflictType,
            Description = description,
            SourceDataJson = earlier.NewDataJson ?? earlier.OldDataJson ?? "",
            TargetDataJson = later.NewDataJson ?? later.OldDataJson ?? "",
            SourceSiteId = earlier.SourceSiteId,
            TargetSiteId = later.SourceSiteId,
            ConflictDetectedAt = DateTime.UtcNow,
            Priority = GetConflictPriority(conflictType),
            ResolutionStrategy = ConflictResolutionStrategy.LastWriteWins,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private string? DetermineConflictType(ChangeRecordDto change1, ChangeRecordDto change2)
    {
        if (change1.Operation == ChangeOperation.Delete && change2.Operation == ChangeOperation.Update)
            return "DeleteUpdateConflict";
        
        if (change1.Operation == ChangeOperation.Update && change2.Operation == ChangeOperation.Delete)
            return "UpdateDeleteConflict";
        
        if (change1.Operation == ChangeOperation.Update && change2.Operation == ChangeOperation.Update)
        {
            if (!string.Equals(change1.NewDataJson, change2.NewDataJson, StringComparison.OrdinalIgnoreCase))
                return "DataMismatch";
        }

        return null;
    }

    private string GetConflictDescription(string conflictType)
    {
        return conflictType switch
        {
            "DeleteUpdateConflict" => "Record was deleted in one site but updated in another",
            "UpdateDeleteConflict" => "Record was updated in one site but deleted in another",
            "DataMismatch" => "Same record updated differently in multiple sites",
            _ => "Conflict detected between changes"
        };
    }

    private int GetConflictPriority(string conflictType)
    {
        return conflictType switch
        {
            "DeleteUpdateConflict" => 9,
            "UpdateDeleteConflict" => 9,
            "DataMismatch" => 5,
            _ => 3
        };
    }
}