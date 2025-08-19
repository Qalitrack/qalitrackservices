/*using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Contracts.Messaging.Commands;
using UserService.Core.Contracts.Messaging.Commands.Commands;
using UserService.Core.Enums;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Backup;
using UserService.Core.Interfaces.Messaging;
using UserService.Core.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

[AllowAnonymous]
public class BackupController : ControllerBase
{
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<BackupController> _logger;
    private readonly IBackupCommandPublisher _commandPublisher;

    public BackupController(
        IDatabaseBackupService backupService,
        ILogger<BackupController> logger,
        IBackupCommandPublisher commandPublisher)
    {
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandPublisher = commandPublisher ?? throw new ArgumentNullException(nameof(commandPublisher));
    }

    [HttpPost("full")]
    public async Task<IActionResult> CreateFullBackup([FromQuery] bool async = false)
    {
        try
        {
            if (async)
            {
                var correlationId = Guid.NewGuid();
                await _commandPublisher.PublishCreateBackupCommand(
                    new CreateBackupCommand(correlationId, DateTime.UtcNow, BackupType.Full.ToString()));
                
                return Accepted(new 
                {
                    Message = "Full backup request accepted",
                    CorrelationId = correlationId,
                    Type = "Full",
                    Status = "Processing"
                });
            }

            var backupPath = await _backupService.CreateBackupAsync(BackupType.Full);
            return Ok(new 
            { 
                Message = "Full backup created successfully",
                Path = backupPath,
                Type = "Full"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating full backup");
            return StatusCode(500, new { Error = "Error creating full backup", Details = ex.Message });
        }
    }

    [HttpPost("incremental")]
    public async Task<IActionResult> CreateIncrementalBackup([FromQuery] bool async = false)
    {
        try
        {
            if (async)
            {
                var correlationId = Guid.NewGuid();
                await _commandPublisher.PublishCreateBackupCommand(
                    new CreateBackupCommand(correlationId, DateTime.UtcNow, BackupType.Incremental.ToString()));
                
                return Accepted(new 
                {
                    Message = "Incremental backup request accepted",
                    CorrelationId = correlationId,
                    Type = "Incremental",
                    Status = "Processing"
                });
            }

            var backupPath = await _backupService.CreateBackupAsync(BackupType.Incremental);
            return Ok(new 
            { 
                Message = "Incremental backup created successfully",
                Path = backupPath,
                Type = "Incremental"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incremental backup");
            return StatusCode(500, new { Error = "Error creating incremental backup", Details = ex.Message });
        }
    }

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreDatabase(
        [FromQuery] string backupPath,
        [FromQuery] bool async = false)
    {
        if (string.IsNullOrEmpty(backupPath))
        {
            return BadRequest(new { Error = "Backup path is required" });
        }

        try
        {
            if (async)
            {
                var correlationId = Guid.NewGuid();
                await _commandPublisher.PublishRestoreBackupCommand(
                    new RestoreBackupCommand(correlationId, backupPath, DateTime.UtcNow));
                
                return Accepted(new 
                {
                    Message = $"Restore request accepted for {backupPath}",
                    CorrelationId = correlationId,
                    Status = "Processing"
                });
            }

            // Assuming your IDatabaseBackupService has a RestoreBackupAsync method
            await _backupService.RestoreBackupAsync(backupPath);
            return Ok(new 
            { 
                Message = $"Database restored from {backupPath}"
            });
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Backup file not found: {BackupPath}", backupPath);
            return NotFound(new { Error = $"Backup file not found: {backupPath}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring database from {BackupPath}", backupPath);
            return StatusCode(500, new { Error = "Error restoring database", Details = ex.Message });
        }
    }

    [HttpGet("status/{correlationId}")]
    public async Task<IActionResult> GetBackupStatus(Guid correlationId)
    {
        try
        {
            // Assuming you'll implement this in your backup service
            var status = await _backupService.GetBackupStatusAsync(correlationId);
            
            return Ok(status);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = $"No backup found with correlation ID {correlationId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for {CorrelationId}", correlationId);
            return StatusCode(500, new { Error = "Error getting backup status", Details = ex.Message });
        }
    }
}*/