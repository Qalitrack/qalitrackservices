using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackupService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;
        private readonly IMicroService _microserviceService;
        private readonly ILogger<BackupController> _logger;
        private readonly IBackupRestoreService _backupRestoreService;

        public BackupController(
            IDatabaseBackupService backupService,
            IBackupRestoreService backupRestoreService,
            IMicroService microserviceService,
            ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _microserviceService = microserviceService;
            _logger = logger;
            _backupRestoreService = backupRestoreService;
        }

        /// <summary>
        /// Create a new backup for a microservice
        /// </summary>
        /// <param name="request">Backup creation request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Backup result</returns>
        [HttpPost("create")]
        public async Task<ActionResult<BackupResult>> CreateBackup([FromBody] BackupRequest request, CancellationToken ct = default)
        {
            try
            {
                // Validate microservice exists
                var microservice = await _microserviceService.GetMicroserviceAsync(request.Microservice, ct);
                
                var result = await _backupService.CreateBackupAsync(
                    request.Type,
                    request.Microservice,
                    request.SaveLocation,
                    request.CronSchedule,
                    ct);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Microservice not found: {Microservice}", request.Microservice);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot create backup for microservice: {Microservice}", request.Microservice);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup for microservice: {Microservice}", request.Microservice);
                return StatusCode(500, new { error = "Internal server error while creating backup" });
            }
        }

        /// <summary>
        /// Restore a backup for a microservice
        /// </summary>
        /// <param name="request">Restore request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Restore result</returns>
        [HttpPost("restore")]
public async Task<ActionResult<RestoreResult>> RestoreBackup([FromBody] RestoreBackupRequest request, CancellationToken ct = default)
{
    try
    {
    

        // Validate microservice exists
        var microservice = await _microserviceService.GetMicroserviceAsync(request.Microservice, ct);
        if (microservice == null)
        {
            throw new KeyNotFoundException($"Microservice {request.Microservice} not found");
        }

        // Validate that backup source path is provided
        if (string.IsNullOrWhiteSpace(request.BackupSourcePath))
        {
            throw new ArgumentException("BackupSourcePath is required for restore operation");
        }

        // Determine the backup file path
        string backupFilePath;
        
        if (System.IO.File.Exists(request.BackupSourcePath))
        {
            // BackupSourcePath is already a full file path
            backupFilePath = request.BackupSourcePath;
        }
        else if (System.IO.Directory.Exists(request.BackupSourcePath))
        {
            // BackupSourcePath is a directory, treat BackupId as filename
            backupFilePath = System.IO.Path.Combine(request.BackupSourcePath, request.BackupId);
            
            // If BackupId doesn't have extension, try adding common extensions
            if (!System.IO.File.Exists(backupFilePath))
            {
                var extensions = new[] { ".dump", ".sql" };
                foreach (var ext in extensions)
                {
                    var fileWithExt = backupFilePath + ext;
                    if (System.IO.File.Exists(fileWithExt))
                    {
                        backupFilePath = fileWithExt;
                        break;
                    }
                }
            }
        }
        else
        {
            throw new DirectoryNotFoundException($"Backup source path not found: {request.BackupSourcePath}");
        }

        // Final validation that the backup file exists
        if (!System.IO.File.Exists(backupFilePath))
        {
            throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
        }

        _logger.LogInformation("Using backup file: {BackupFilePath}", backupFilePath);

        // Call the service method with the backup file path directly
        var result = await _backupService.RestoreBackupAsync(
            request.Microservice,
            backupFilePath,
            ct);

        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Microservice or backup not found: {Microservice}, {BackupId}", 
            request.Microservice, request.BackupId);
        return NotFound(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Invalid arguments for restore: {Microservice}", request.Microservice);
        return BadRequest(new { error = ex.Message });
    }
    catch (DirectoryNotFoundException ex)
    {
        _logger.LogWarning(ex, "Backup directory not found: {BackupSourcePath}", request.BackupSourcePath);
        return NotFound(new { error = ex.Message });
    }
    catch (FileNotFoundException ex)
    {
        _logger.LogWarning(ex, "Backup file not found: {BackupFilePath}", ex.FileName);
        return NotFound(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error restoring backup for microservice: {Microservice}", request.Microservice);
        return StatusCode(500, new { error = "Internal server error while restoring backup" });
    }
}



        /// <summary>
        /// <summary>
        /// Get available backups
        /// </summary>
        /// <param name="microservice">Optional microservice filter</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of available backups</returns>
        [HttpGet("available")]
        public async Task<ActionResult<List<BackupFileInfo>>> GetAvailableBackups(
            [FromQuery] string? microservice = null, 
            CancellationToken ct = default)
        {
            try
            {
        
                var backups = await _backupService.GetAvailableBackupsAsync(microservice, ct);
                return Ok(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available backups for microservice: {Microservice}", microservice);
                return StatusCode(500, new { error = "Internal server error while retrieving available backups" });
            }
        }

      


        /// <summary>
        /// Get backup statistics
        /// </summary>
        /// <param name="microservice">Optional microservice filter</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Backup statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<BackupStatistics>> GetBackupStatistics(
            [FromQuery] string? microservice = null, 
            CancellationToken ct = default)
        {
            try
            {

                var statistics = await _backupService.GetBackupStatisticsAsync(microservice, ct);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup statistics for microservice: {Microservice}", microservice);
                return StatusCode(500, new { error = "Internal server error while retrieving backup statistics" });
            }
        }

        /// <summary>
        /// Get scheduled backups
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of scheduled backups</returns>
        [HttpGet("scheduled")]
        public async Task<ActionResult<List<ScheduledBackup>>> GetScheduledBackups(CancellationToken ct = default)
        {
            try
            {

                var scheduledBackups = await _backupService.GetScheduledBackupsAsync(ct);
                return Ok(scheduledBackups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduled backups");
                return StatusCode(500, new { error = "Internal server error while retrieving scheduled backups" });
            }
        }

        /// <summary>
        /// Unschedule a backup job
        /// </summary>
        /// <param name="microservice">Microservice name</param>
        /// <param name="backupType">Backup type</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Unschedule result</returns>
        [HttpDelete("unschedule/{microservice}/{backupType}")]
        public async Task<ActionResult> UnscheduleBackup(
            [FromRoute] string microservice,
            [FromRoute] BackupType backupType,
            CancellationToken ct = default)
        {
            try
            {

                var result = await _backupService.UnscheduleBackupAsync(microservice, backupType, ct);
                
                if (result)
                {
                    return Ok(new { message = $"Successfully unscheduled {backupType} backup for {microservice}" });
                }
                else
                {
                    return NotFound(new { error = $"No scheduled {backupType} backup found for {microservice}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unscheduling {BackupType} backup for microservice: {Microservice}", 
                    backupType, microservice);
                return StatusCode(500, new { error = "Internal server error while unscheduling backup" });
            }
        }
    }
}