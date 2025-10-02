using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Enums;
using BackupService.Core.Interfaces;
using BackupService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BackupService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;
        private readonly IMicroService _microserviceService;
        private readonly ILogger<BackupController> _logger;
        private readonly IBackupRestoreService _backupRestoreService;
        private readonly IBackupMetadataService _backupMetadataService;
        private readonly IConfiguration _configuration;

        public BackupController(
            IDatabaseBackupService backupService,
            IBackupRestoreService backupRestoreService,
            IBackupMetadataService backupMetadataService,
            IMicroService microserviceService,
            ILogger<BackupController> logger,
            IConfiguration configuration)
        {
            _backupService = backupService;
            _microserviceService = microserviceService;
            _logger = logger;
            _backupRestoreService = backupRestoreService;
            _backupMetadataService = backupMetadataService;
            _configuration = configuration;
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
                    (BackupType)request.Type,  // Use the type from the request
                    request.Microservice,
                    request.CronSchedule,      // Pass the cron schedule from the request
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
        /// <param name="request">Restore request containing microservice and backup ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Restore result</returns>
        /// <summary>
        /// Download a backup file by ID and microservice name
        /// </summary>
        /// <param name="microservice">Name of the microservice</param>
        /// <param name="backupId">ID of the backup to download (use 'latest' for most recent)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Backup file as a downloadable response</returns>
        [HttpGet("download/{microservice}/{backupId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadBackup(string microservice, string backupId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(microservice))
                {
                    throw new ArgumentException("Microservice name is required");
                }

                if (string.IsNullOrWhiteSpace(backupId))
                {
                    backupId = "latest"; // Default to 'latest' if not specified
                }

                // Get available backups for the microservice
                var backups = await _backupService.GetAvailableBackupsAsync(microservice, ct);
                
                // If backupId is 'latest', get the most recent backup, otherwise find by ID
                var backupInfo = backupId == "latest" 
                    ? backups.OrderByDescending(b => b.CreatedAt).FirstOrDefault()
                    : backups.FirstOrDefault(b => b.BackupId == backupId);

                if (backupInfo == null)
                {
                    throw new KeyNotFoundException($"Backup with ID '{backupId}' not found for microservice '{microservice}'");
                }

                string backupFilePath = backupInfo.FileName;

                if (string.IsNullOrEmpty(backupFilePath) || !System.IO.File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
                }

                _logger.LogInformation("Downloading backup for microservice: {Microservice}, Backup ID: {BackupId}, File: {BackupFilePath}", 
                    microservice, backupId, backupFilePath);

                // Open the file stream and return it as a downloadable response
                var fileStream = System.IO.File.OpenRead(backupFilePath);
                return File(fileStream, "application/octet-stream", Path.GetFileName(backupFilePath));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Microservice or backup not found: {Microservice}, {BackupId}", 
                    microservice, backupId);
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments for download: {Microservice}", microservice);
                return BadRequest(new { error = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Backup file not found: {BackupFilePath}", ex.FileName);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backup. Microservice: {Microservice}, BackupId: {BackupId}", 
                    microservice, backupId);
                return StatusCode(500, new { error = "An error occurred while downloading the backup" });
            }
        }

        [HttpPost("restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestoreResult>> RestoreBackup([FromBody] RestoreBackupRequest request, CancellationToken ct = default)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Microservice))
                {
                    throw new ArgumentException("Microservice name is required");
                }

                if (string.IsNullOrWhiteSpace(request.BackupId))
                {
                    request.BackupId = "latest"; // Default to 'latest' if not specified
                }

                // Get available backups for the microservice to find the file path by ID
                var backups = await _backupService.GetAvailableBackupsAsync(request.Microservice, ct);
                
                // If backupId is 'latest', get the most recent backup, otherwise find by ID
                var backupInfo = request.BackupId == "latest" 
                    ? backups.OrderByDescending(b => b.CreatedAt).FirstOrDefault()
                    : backups.FirstOrDefault(b => b.BackupId == request.BackupId);

                if (backupInfo == null)
                {
                    throw new KeyNotFoundException($"Backup with ID '{request.BackupId}' not found for microservice '{request.Microservice}'");
                }

                string backupFilePath = backupInfo.FileName;

                if (string.IsNullOrEmpty(backupFilePath) || !System.IO.File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
                }

                _logger.LogInformation("Restoring backup for microservice: {Microservice}, Backup ID: {BackupId}, File: {BackupFilePath}", 
                    request.Microservice, request.BackupId, backupFilePath);

                // Call the service method with the backup file path
                var restoreResult = await _backupService.RestoreBackupAsync(
                    request.Microservice,
                    backupFilePath,
                    ct);

                return Ok(restoreResult);
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
                _logger.LogWarning(ex, "Backup directory not found for microservice: {Microservice}", request.Microservice);
                return NotFound(new { error = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Backup file not found: {BackupFilePath}", ex.FileName);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for restore: {Microservice}, {BackupId}", request.Microservice, request.BackupId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup for microservice: {Microservice}", request.Microservice);
                return StatusCode(500, new { error = "Internal server error while restoring backup" });
            }
        }

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
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "No backups found for microservice: {Microservice}", microservice);
                return NotFound(new { error = ex.Message });
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
        public async Task<ActionResult<List<ScheduledBackup>>> GetScheduledBackups([FromQuery] string? microservice = null, CancellationToken ct = default)
        {
            try
            {

                var scheduledBackups = await _backupService.GetScheduledBackupsAsync(ct);
                if (!string.IsNullOrEmpty(microservice))
                {
                    scheduledBackups = scheduledBackups.Where(b => b.Microservice == microservice).ToList();
                }
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