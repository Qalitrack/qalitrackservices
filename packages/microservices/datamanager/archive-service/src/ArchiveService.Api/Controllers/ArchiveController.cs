using Microsoft.AspNetCore.Mvc;
using ArchiveService.Core.DTOs;
using ArchiveService.Core.Interfaces;

namespace ArchiveService.Api.Controllers
{
    /// <summary>
    /// Archive Management Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Archive Management")]
    public class ArchiveController : BaseController
    {
        private readonly IArchivalEngine _archivalEngine;
        private readonly ILogger<ArchiveController> _logger;

        public ArchiveController(IArchivalEngine archivalEngine, ILogger<ArchiveController> logger)
        {
            _archivalEngine = archivalEngine;
            _logger = logger;
        }

        /// <summary>
        /// Archive transactions by date range
        /// </summary>
        [HttpPost("transactions")]
        public async Task<IActionResult> ArchiveTransactions([FromBody] ArchiveRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Archiving transactions for date range {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                
                request.RequestedBy = GetUserId();
                var result = await _archivalEngine.ArchiveTransactionsAsync(request, cancellationToken);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequestWithMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving transactions");
                return HandleException(ex, "Failed to archive transactions");
            }
        }

        /// <summary>
        /// Archive weight measurements by date range
        /// </summary>
        [HttpPost("measurements")]
        public async Task<IActionResult> ArchiveWeightMeasurements([FromBody] ArchiveRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Archiving weight measurements for date range {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                
                request.RequestedBy = GetUserId();
                var result = await _archivalEngine.ArchiveWeightMeasurementsAsync(request, cancellationToken);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequestWithMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving weight measurements");
                return HandleException(ex, "Failed to archive weight measurements");
            }
        }

        /// <summary>
        /// Archive compliance records by date range
        /// </summary>
        [HttpPost("compliance")]
        public async Task<IActionResult> ArchiveComplianceRecords([FromBody] ArchiveRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Archiving compliance records for date range {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                
                request.RequestedBy = GetUserId();
                var result = await _archivalEngine.ArchiveComplianceRecordsAsync(request, cancellationToken);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequestWithMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving compliance records");
                return HandleException(ex, "Failed to archive compliance records");
            }
        }

        /// <summary>
        /// Get archival status for a specific archive
        /// </summary>
        [HttpGet("status/{archiveId}")]
        public async Task<IActionResult> GetArchiveStatus(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _archivalEngine.GetArchiveStatusAsync(archiveId, cancellationToken);
                return HandleResult(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting archive status for {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to get archive status");
            }
        }

        /// <summary>
        /// Get archive operation history
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetArchiveHistory(
            [FromQuery] string entityType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequestWithMessage("Entity type is required");
                }

                var result = await _archivalEngine.GetArchiveHistoryAsync(entityType, startDate, endDate, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting archive history for entity type {EntityType}", entityType);
                return HandleException(ex, "Failed to get archive history");
            }
        }

        /// <summary>
        /// Delete archived data
        /// </summary>
        [HttpDelete("{archiveId}")]
        public async Task<IActionResult> DeleteArchive(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _archivalEngine.DeleteArchivedDataAsync(archiveId, cancellationToken);
                
                if (result)
                {
                    return Ok(new { message = "Archive deleted successfully" });
                }
                
                return NotFound(new { error = "Archive not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to delete archive");
            }
        }

        /// <summary>
        /// Validate archive integrity
        /// </summary>
        [HttpPost("validate/{archiveId}")]
        public async Task<IActionResult> ValidateArchiveIntegrity(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _archivalEngine.ValidateArchiveIntegrityAsync(archiveId, cancellationToken);
                
                return Ok(new { 
                    archiveId = archiveId,
                    isValid = result,
                    message = result ? "Archive integrity is valid" : "Archive integrity validation failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating archive integrity for {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to validate archive integrity");
            }
        }

        /// <summary>
        /// Export archived data
        /// </summary>
        [HttpPost("export/{archiveId}")]
        public async Task<IActionResult> ExportArchive(
            string archiveId,
            [FromQuery] string format = "JSON",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _archivalEngine.ExportArchiveAsync(archiveId, format, cancellationToken);
                
                var contentType = format.ToUpper() switch
                {
                    "JSON" => "application/json",
                    "CSV" => "text/csv",
                    "XML" => "application/xml",
                    _ => "application/octet-stream"
                };

                var fileName = $"archive_{archiveId}_{DateTime.UtcNow:yyyyMMdd}.{format.ToLower()}";
                
                return File(data, contentType, fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to export archive");
            }
        }

        /// <summary>
        /// Get archive size
        /// </summary>
        [HttpGet("size/{archiveId}")]
        public async Task<IActionResult> GetArchiveSize(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var size = await _archivalEngine.GetArchiveSizeAsync(archiveId, cancellationToken);
                
                return Ok(new { 
                    archiveId = archiveId,
                    sizeBytes = size,
                    sizeMB = Math.Round(size / 1024.0 / 1024.0, 2),
                    sizeGB = Math.Round(size / 1024.0 / 1024.0 / 1024.0, 4)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting archive size for {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to get archive size");
            }
        }
    }
}