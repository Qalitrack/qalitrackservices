using Microsoft.Extensions.Logging;
using System.Text.Json;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;

namespace DataSyncService.Infrastructure.Services;

public class ConflictResolver : IConflictResolver
{
    private readonly ILogger<ConflictResolver> _logger;

    public ConflictResolver(ILogger<ConflictResolver> logger)
    {
        _logger = logger;
    }

    public async Task<ConflictResolutionResponse> ResolveConflictAsync(
        SyncConflictDto conflict, 
        ConflictResolutionStrategy strategy, 
        string? resolvedDataJson = null)
    {
        try
        {
            _logger.LogInformation("Resolving conflict {ConflictId} using strategy {Strategy}", 
                conflict.ConflictId, strategy);

            string resolvedData = strategy switch
            {
                ConflictResolutionStrategy.LastWriteWins => await ApplyLastWriteWinsAsync(conflict),
                ConflictResolutionStrategy.FirstWriteWins => await ApplyFirstWriteWinsAsync(conflict),
                ConflictResolutionStrategy.MergeChanges => await ApplyMergeChangesAsync(conflict),
                ConflictResolutionStrategy.Manual or ConflictResolutionStrategy.UserDefined => 
                    resolvedDataJson ?? await ApplyLastWriteWinsAsync(conflict),
                _ => await ApplyLastWriteWinsAsync(conflict)
            };

            var isValid = await ValidateResolutionAsync(conflict, resolvedData);
            if (!isValid)
            {
                return new ConflictResolutionResponse
                {
                    Success = false,
                    Message = "Resolution validation failed"
                };
            }

            conflict.ResolvedDataJson = resolvedData;
            conflict.ResolutionStrategy = strategy;
            conflict.Status = ConflictStatus.Resolved;
            conflict.ResolvedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully resolved conflict {ConflictId}", conflict.ConflictId);

            return new ConflictResolutionResponse
            {
                Success = true,
                Message = "Conflict resolved successfully",
                ResolvedConflict = conflict
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving conflict {ConflictId}", conflict.ConflictId);
            return new ConflictResolutionResponse
            {
                Success = false,
                Message = $"Error resolving conflict: {ex.Message}"
            };
        }
    }

    public async Task<string> ApplyLastWriteWinsAsync(SyncConflictDto conflict)
    {
        try
        {
            _logger.LogDebug("Applying last write wins for conflict {ConflictId}", conflict.ConflictId);
            
            // Return the target data (most recent change)
            return conflict.TargetDataJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying last write wins for conflict {ConflictId}", conflict.ConflictId);
            throw;
        }
    }

    public async Task<string> ApplyFirstWriteWinsAsync(SyncConflictDto conflict)
    {
        try
        {
            _logger.LogDebug("Applying first write wins for conflict {ConflictId}", conflict.ConflictId);
            
            // Return the source data (first change)
            return conflict.SourceDataJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying first write wins for conflict {ConflictId}", conflict.ConflictId);
            throw;
        }
    }

    public async Task<string> ApplyMergeChangesAsync(SyncConflictDto conflict)
    {
        try
        {
            _logger.LogDebug("Applying merge changes for conflict {ConflictId}", conflict.ConflictId);

            if (string.IsNullOrEmpty(conflict.SourceDataJson) || string.IsNullOrEmpty(conflict.TargetDataJson))
            {
                return conflict.TargetDataJson ?? conflict.SourceDataJson;
            }

            // Parse JSON data
            var sourceData = JsonDocument.Parse(conflict.SourceDataJson);
            var targetData = JsonDocument.Parse(conflict.TargetDataJson);

            // Create merged object
            var mergedData = new Dictionary<string, object?>();

            // Add all properties from source
            foreach (var property in sourceData.RootElement.EnumerateObject())
            {
                mergedData[property.Name] = GetJsonValue(property.Value);
            }

            // Override/add properties from target
            foreach (var property in targetData.RootElement.EnumerateObject())
            {
                var targetValue = GetJsonValue(property.Value);
                
                // Skip null values from target unless source doesn't have the property
                if (targetValue != null || !mergedData.ContainsKey(property.Name))
                {
                    mergedData[property.Name] = targetValue;
                }
            }

            var mergedJson = JsonSerializer.Serialize(mergedData, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            _logger.LogDebug("Successfully merged changes for conflict {ConflictId}", conflict.ConflictId);
            
            return mergedJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging changes for conflict {ConflictId}", conflict.ConflictId);
            
            // Fallback to last write wins
            return await ApplyLastWriteWinsAsync(conflict);
        }
    }

    public async Task<string> ApplyCustomResolutionAsync(SyncConflictDto conflict, string resolvedDataJson)
    {
        try
        {
            _logger.LogDebug("Applying custom resolution for conflict {ConflictId}", conflict.ConflictId);

            // Validate that the provided JSON is valid
            JsonDocument.Parse(resolvedDataJson);
            
            return resolvedDataJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying custom resolution for conflict {ConflictId}", conflict.ConflictId);
            throw;
        }
    }

    public async Task<bool> ValidateResolutionAsync(SyncConflictDto conflict, string resolvedDataJson)
    {
        try
        {
            if (string.IsNullOrEmpty(resolvedDataJson))
            {
                _logger.LogWarning("Resolved data is empty for conflict {ConflictId}", conflict.ConflictId);
                return false;
            }

            // Validate JSON format
            try
            {
                JsonDocument.Parse(resolvedDataJson);
            }
            catch (JsonException)
            {
                _logger.LogError("Invalid JSON format in resolution for conflict {ConflictId}", conflict.ConflictId);
                return false;
            }

            // Additional validation logic could include:
            // - Business rule validation
            // - Data type validation
            // - Required field validation
            // - Cross-reference validation

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating resolution for conflict {ConflictId}", conflict.ConflictId);
            return false;
        }
    }

    private object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longVal) ? longVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(GetJsonValue).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValue(p.Value)),
            _ => element.ToString()
        };
    }
}