using Microsoft.AspNetCore.Mvc;
using Transaction.Core.DTOs;
using Transaction.Core.Services;

namespace Transaction.Api.Controllers;

[Route("Settings")]
public class TransactionSettingsController : BaseController
{
    private readonly ITransactionSettingsService _settingsService;
    private readonly ILogger<TransactionSettingsController> _logger;

    public TransactionSettingsController(
        ITransactionSettingsService settingsService,
        ILogger<TransactionSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current transaction settings (e.g. ticket/receipt number prefix)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction settings");
            return InternalServerError("An error occurred while retrieving transaction settings");
        }
    }

    /// <summary>
    /// Update the transaction settings (e.g. ticket/receipt number prefix)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateTransactionSettingsDto dto)
    {
        try
        {
            var settings = await _settingsService.UpdateSettingsAsync(dto);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction settings");
            return InternalServerError("An error occurred while updating transaction settings");
        }
    }
}
