using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("devicemanagement/php/lane2")]
public class Plate2ReceiverController : ControllerBase
{
    private readonly PlateDataStreamService _plateDataStreamService;
    private readonly ILogger<Plate2ReceiverController> _logger;

    public Plate2ReceiverController(
        [FromKeyedServices("lane2")] PlateDataStreamService plateDataStreamService,
        ILogger<Plate2ReceiverController> logger)
    {
        _plateDataStreamService = plateDataStreamService;
        _logger = logger;
    }

    [HttpPost("plateresult.php")]
    public async Task<IActionResult> ReceivePlateResult()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           PLATE DATA RECEIVED FROM CAMERA — LANE 2             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
            Console.WriteLine($"Source IP: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Content-Type: {Request.ContentType}");
            Console.WriteLine($"Content-Length: {Request.ContentLength}");
            Console.WriteLine("\n--- Headers ---");
            foreach (var header in Request.Headers)
                Console.WriteLine($"{header.Key}: {header.Value}");
            Console.WriteLine("\n--- Raw Body ---");
            Console.WriteLine(body);
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("Received empty plate data from camera (lane 2)");
                return Ok(new { status = "received", message = "empty_body" });
            }

            object plateData;
            try
            {
                plateData = JsonSerializer.Deserialize<JsonElement>(body);
                var prettyJson = JsonSerializer.Serialize(plateData, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- Parsed JSON ---");
                Console.WriteLine(prettyJson);
                Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            }
            catch (JsonException)
            {
                _logger.LogInformation("Received non-JSON plate data (lane 2): {body}", body);
                plateData = new { rawData = body, timestamp = DateTime.UtcNow };
            }

            await _plateDataStreamService.PublishPlateAsync(plateData);

            var clientCount = _plateDataStreamService.GetClientCount();
            _logger.LogInformation("Lane 2 plate data published to {clientCount} clients", clientCount);

            if (clientCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ [Lane 2] Broadcasted to {clientCount} connected client(s)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ [Lane 2] No clients connected to stream (waiting at /api/plates/lane2/stream)");
                Console.ResetColor();
            }

            return Ok(new
            {
                status = "success",
                lane = 2,
                received = DateTime.UtcNow,
                clientsNotified = clientCount,
                license = plateData.GetType().GetProperty("license")?.GetValue(plateData, null)?.ToString() ?? "unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 plate data");
            Console.WriteLine($"\n❌ [Lane 2] ERROR processing plate data: {ex.Message}\n");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("quickplateresult.php")]
    public async Task<IActionResult> ReceiveQuickPlateResult()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("\n⚡ [Lane 2] QUICK Plate Result:");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine($"Source: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Data: {body}\n");

            if (!string.IsNullOrWhiteSpace(body))
            {
                object plateData;
                try { plateData = JsonSerializer.Deserialize<JsonElement>(body); }
                catch { plateData = new { rawData = body, timestamp = DateTime.UtcNow }; }
                await _plateDataStreamService.PublishPlateAsync(plateData);
            }

            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 quick plate result");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("receivedeviceinfo.php")]
    public async Task<IActionResult> ReceiveDeviceInfo()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"\n📡 [Lane 2] Device Info from {HttpContext.Connection.RemoteIpAddress}: {body}\n");
            _logger.LogDebug("Lane 2 device info: {body}", body);
            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 device info");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("gio.php")]
    public async Task<IActionResult> ReceiveGpioTrigger()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"\n🔌 [Lane 2] GPIO Trigger: {body}");
            _logger.LogDebug("Lane 2 GPIO trigger: {body}", body);
            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 GPIO trigger");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("porttrigger.php")]
    public async Task<IActionResult> ReceivePortTrigger()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"\n🔔 [Lane 2] Port Trigger: {body}");
            _logger.LogDebug("Lane 2 port trigger: {body}", body);
            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 port trigger");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("gate.php")]
    public async Task<IActionResult> ReceiveGateStatus()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"\n🚧 [Lane 2] Gate Status: {body}");
            _logger.LogDebug("Lane 2 gate status: {body}", body);
            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 gate status");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("serial.php")]
    public async Task<IActionResult> ReceiveSerialData()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"\n📟 [Lane 2] Serial Data: {body}");
            _logger.LogDebug("Lane 2 serial data: {body}", body);
            return Ok(new { status = "success", lane = 2 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing lane 2 serial data");
            return Ok(new { status = "error", message = ex.Message });
        }
    }
}
