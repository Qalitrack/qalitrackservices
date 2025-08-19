using BackupService.Core.Dtos;
using BackupService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Messaging.Contracts.Messaging.contracts.Enums;
using System;
using System.Threading.Tasks;

namespace BackupService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupInitiationService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(
        IBackupInitiationService backupService,
        ILogger<BackupController> logger)
    {
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates a backup for all services
    /// </summary>
    /// <param name="backupType">Type of backup to perform (Full or Incremental)</param>
    /// <returns>Accepted response with correlation ID</returns>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateBackupForAllServices([FromQuery] BackupType backupType = BackupType.Full)
    {
        _logger.LogInformation("Received request to initiate {BackupType} backup for all services", backupType);

        try
        {
            var commandType = backupType == BackupType.Full ? BackupCommandType.CreateFullBackup : BackupCommandType.CreateIncrementalBackup;
            var correlationId = await _backupService.InitiateBackupCommandAsync(commandType);

            _logger.LogInformation("Broadcast {BackupType} backup initiated with correlation ID {CorrelationId}", 
                backupType, correlationId);

            return Accepted(new ApiResponseDto<object>
            { 
                Success = true,
                Message = $"{backupType} backup initiated for all services",
                Data = new 
                { 
                    correlationId = correlationId,
                    backupType = backupType.ToString(),
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate {BackupType} backup for all services", backupType);

            return StatusCode(500, new ApiResponseDto<object> 
            { 
                Success = false,
                Message = "Failed to initiate backup",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Initiates a restore operation for a specific backup
    /// </summary>
    /// <param name="backupId">The ID of the backup to restore (default: "latest")</param>
    /// <returns>Accepted response with correlation ID</returns>
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreBackup([FromQuery] string backupId = "latest")
    {
        _logger.LogInformation("Received request to restore backup with ID {BackupId}", backupId);

        try
        {
            var correlationId = await _backupService.InitiateBackupCommandAsync(BackupCommandType.RestoreBackup, chainId: backupId);

            _logger.LogInformation("Broadcast restore request initiated with correlation ID {CorrelationId} for backup ID {BackupId}", 
                correlationId, backupId);

            return Accepted(new ApiResponseDto<object>
            {
                Success = true,
                Message = $"Restore initiated for backup ID {backupId}",
                Data = new
                {
                    correlationId = correlationId,
                    backupId = backupId,
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate restore for backup ID {BackupId}", backupId);

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to initiate restore",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Lists all available backup chains
    /// </summary>
    /// <returns>List of backup chain information</returns>
    [HttpGet("list")]
    public async Task<IActionResult> ListBackups()
    {
        _logger.LogInformation("Received request to list all backup chains");

        try
        {
            var correlationId = await _backupService.InitiateBackupCommandAsync(BackupCommandType.ListBackups);

            _logger.LogInformation("Broadcast list backups request initiated with correlation ID {CorrelationId}", correlationId);

            return Accepted(new ApiResponseDto<object>
            {
                Success = true,
                Message = "List backups request initiated",
                Data = new
                {
                    correlationId = correlationId,
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate list backups request");

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to initiate list backups",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Previews a restore operation for a specific backup
    /// </summary>
    /// <param name="backupId">The ID of the backup to preview (default: "latest")</param>
    /// <returns>Accepted response with correlation ID</returns>
    [HttpPost("preview-restore")]
    public async Task<IActionResult> PreviewRestore([FromQuery] string backupId = "latest")
    {
        _logger.LogInformation("Received request to preview restore for backup ID {BackupId}", backupId);

        try
        {
            var correlationId = await _backupService.InitiateBackupCommandAsync(BackupCommandType.PreviewRestore, chainId: backupId);

            _logger.LogInformation("Broadcast preview restore request initiated with correlation ID {CorrelationId} for backup ID {BackupId}", 
                correlationId, backupId);

            return Accepted(new ApiResponseDto<object>
            {
                Success = true,
                Message = $"Restore preview initiated for backup ID {backupId}",
                Data = new
                {
                    correlationId = correlationId,
                    backupId = backupId,
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate preview restore for backup ID {BackupId}", backupId);

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to initiate preview restore",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Verifies the integrity of all backups
    /// </summary>
    /// <returns>Accepted response with correlation ID</returns>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyBackups()
    {
        _logger.LogInformation("Received request to verify all backups");

        try
        {
            var correlationId = await _backupService.InitiateBackupCommandAsync(BackupCommandType.VerifyBackup);

            _logger.LogInformation("Broadcast verify backups request initiated with correlation ID {CorrelationId}", correlationId);

            return Accepted(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Backup verification initiated for all services",
                Data = new
                {
                    correlationId = correlationId,
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate backup verification");

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to initiate backup verification",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Retrieves status of a backup operation by correlation ID
    /// </summary>
    /// <param name="correlationId">The correlation ID of the backup operation</param>
    /// <returns>Backup status information</returns>
    [HttpGet("status/{correlationId:guid}")]
    public async Task<IActionResult> GetBackupStatus(Guid correlationId)
    {
        _logger.LogInformation("Received request for backup status with correlation ID {CorrelationId}", correlationId);

        try
        {
            var result = await _backupService.GetCommandResultAsync(correlationId.ToString(), TimeSpan.FromMinutes(5));

            if (result == null)
            {
                _logger.LogWarning("No backup status found for correlation ID {CorrelationId}", correlationId);
                return NotFound(new ApiResponseDto<object> 
                { 
                    Success = false,
                    Message = $"No backup operation found with correlation ID {correlationId}"
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Backup status retrieved successfully",
                Data = new
                {
                    result.OrchestrationId,
                    result.CommandType,
                    result.IsSuccessful,
                    result.Message,
                    result.StartedAt,
                    result.CompletedAt,
                    ServiceResponses = result.ServiceResponses.Select(r => new
                    {
                        r.Value.ServiceName,
                        r.Value.IsSuccessful,
                        r.Value.Message,
                        r.Value.ProcessedAt,
                        r.Value.ErrorDetails,
                        r.Value.BackupResult,
                        r.Value.RestoreResult,
                        r.Value.RestorePreviewResult,
                        r.Value.HealthReport,
                        r.Value.ChainInfo
                    })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backup status for correlation ID {CorrelationId}", correlationId);

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to retrieve backup status",
                Data = new { error = ex.Message }
            });
        }
    }

    /// <summary>
    /// Initiates cleanup of old backups for all services
    /// </summary>
    /// <param name="retentionDays">Optional number of days to keep backups for (overrides default)</param>
    /// <returns>Result of the cleanup request</returns>
    [HttpPost("cleanup")]
    public async Task<IActionResult> CleanupOldBackups([FromQuery] int? retentionDays = null)
    {
        _logger.LogInformation("Received request to cleanup old backups{RetentionDaysMessage}", 
            retentionDays.HasValue ? $" with retention period of {retentionDays} days" : "");

        try
        {
            var correlationId = await _backupService.InitiateBackupCommandAsync(BackupCommandType.CleanupOldBackups);

            _logger.LogInformation("Broadcast cleanup request initiated with correlation ID {CorrelationId}", correlationId);

            return Accepted(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Backup cleanup initiated for all services",
                Data = new
                {
                    correlationId = correlationId,
                    retentionDays = retentionDays,
                    initiatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating backup cleanup");

            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to initiate backup cleanup",
                Data = new { error = ex.Message }
            });
        }
    }
}