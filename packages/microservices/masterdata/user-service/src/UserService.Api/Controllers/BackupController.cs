// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using UserService.Core.Enums;
// using UserService.Core.Interfaces;
// using UserService.Core.Services;
//
// namespace UserService.Api.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// [Authorize(Roles = "Admin")]
// public class BackupController : ControllerBase
// {
//     private readonly IDatabaseBackupService _backupService;
//     private readonly RestoreService _restoreService;
//     private readonly ILogger<BackupController> _logger;
//
//     public BackupController(
//         IDatabaseBackupService backupService,
//         RestoreService restoreService,
//         ILogger<BackupController> logger)
//     {
//         _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
//         _restoreService = restoreService ?? throw new ArgumentNullException(nameof(restoreService));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }
//
//     [HttpPost("full")]
//     public async Task<IActionResult> CreateFullBackup()
//     {
//         try
//         {
//             var backupPath = await _backupService.CreateBackupAsync(BackupType.Full);
//             return Ok(new { 
//                 Message = "Full backup created successfully",
//                 Path = backupPath,
//                 Type = "Full"
//             });
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error creating full backup");
//             return StatusCode(500, "Error creating full backup");
//         }
//     }
//
//     [HttpPost("incremental")]
//     public async Task<IActionResult> CreateIncrementalBackup()
//     {
//         try
//         {
//             var backupPath = await _backupService.CreateBackupAsync(BackupType.Incremental);
//             return Ok(new { 
//                 Message = "Incremental backup created successfully",
//                 Path = backupPath,
//                 Type = "Incremental"
//             });
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error creating incremental backup");
//             return StatusCode(500, "Error creating incremental backup");
//         }
//     }
//
//     [HttpPost("restore")]
//     public async Task<IActionResult> RestoreDatabase([FromQuery] string backupPath)
//     {
//         if (string.IsNullOrEmpty(backupPath))
//         {
//             return BadRequest("Backup path is required");
//         }
//
//         try
//         {
//             await _restoreService.RestoreDatabaseAsync(backupPath);
//             return Ok(new { 
//                 Message = $"Database restored from {backupPath}"
//             });
//         }
//         catch (FileNotFoundException ex)
//         {
//             _logger.LogError(ex, "Backup file not found: {BackupPath}", backupPath);
//             return NotFound($"Backup file not found: {backupPath}");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error restoring database from {BackupPath}", backupPath);
//             return StatusCode(500, $"Error restoring database: {ex.Message}");
//         }
//     }
// }