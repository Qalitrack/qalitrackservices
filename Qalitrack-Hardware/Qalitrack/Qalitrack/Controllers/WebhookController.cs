using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;
using System.Text.Json;

namespace Qalitrack.Controllers;

[ApiController]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly PlateDataStreamService _plateDataStreamService;

    public WebhookController(
        ILogger<WebhookController> logger,
        PlateDataStreamService plateDataStreamService)
    {
        _logger = logger;
        _plateDataStreamService = plateDataStreamService;
    }

    [HttpPost("devicemanagement/php/plateresult.php")]
    public async Task<IActionResult> ReceivePlateResult()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        try
        {
            var plateData = JsonSerializer.Deserialize<JsonElement>(body);
            await _plateDataStreamService.PublishPlateAsync(plateData);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing plate webhook");
            return Ok();
        }
    }
}
