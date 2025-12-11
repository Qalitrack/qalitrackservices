using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("devicemanagement/php")]
public class PlateReceiverController : ControllerBase
{
    private readonly PlateDataStreamService _plateDataStreamService;
    private readonly ILogger<PlateReceiverController> _logger;

    public PlateReceiverController(
        PlateDataStreamService plateDataStreamService,
        ILogger<PlateReceiverController> logger)
    {
        _plateDataStreamService = plateDataStreamService;
        _logger = logger;
    }

    /// <summary>
    /// Receives plate recognition results from the camera
    /// Camera sends POST to: /devicemanagement/php/plateresult.php
    /// </summary>
    [HttpPost("plateresult.php")]
    public async Task<IActionResult> ReceivePlateResult()
    {
        try
        {
            // Read the raw body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Log everything for debugging
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║               PLATE DATA RECEIVED FROM CAMERA                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
            Console.WriteLine($"Source IP: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Content-Type: {Request.ContentType}");
            Console.WriteLine($"Content-Length: {Request.ContentLength}");
            Console.WriteLine("\n--- Headers ---");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }
            Console.WriteLine("\n--- Raw Body ---");
            Console.WriteLine(body);
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("Received empty plate data from camera");
                return Ok(new { status = "received", message = "empty_body" });
            }

            // Try to parse as JSON
            object plateData;
            try
            {
                plateData = JsonSerializer.Deserialize<JsonElement>(body);
                
                // Pretty print the JSON
                var prettyJson = JsonSerializer.Serialize(plateData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                Console.WriteLine("--- Parsed JSON ---");
                Console.WriteLine(prettyJson);
                Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            }
            catch (JsonException)
            {
                // Not JSON, treat as plain text or form data
                _logger.LogInformation("Received non-JSON plate data: {body}", body);
                plateData = new { rawData = body, timestamp = DateTime.UtcNow };
            }

            // Publish to all connected SSE clients
            await _plateDataStreamService.PublishPlateAsync(plateData);

            var clientCount = _plateDataStreamService.GetClientCount();
            _logger.LogInformation("Plate data published to {clientCount} clients", clientCount);

            // Show a more prominent message when clients are listening
            if (clientCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Broadcasted to {clientCount} connected client(s)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ No clients connected to stream (waiting for connections at /api/plates/stream)");
                Console.ResetColor();
            }

            // Return success response to camera
            return Ok(new 
            { 
                status = "success",
                received = DateTime.UtcNow,
                clientsNotified = clientCount,
                license = plateData.GetType().GetProperty("license")?.GetValue(plateData, null)?.ToString() ?? "unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing plate data from camera");
            Console.WriteLine($"\n❌ ERROR processing plate data: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}\n");
            
            // Still return OK to camera so it doesn't retry
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives device info/heartbeat from camera
    /// Camera sends POST to: /devicemanagement/php/receivedeviceinfo.php
    /// </summary>
    [HttpPost("receivedeviceinfo.php")]
    public async Task<IActionResult> ReceiveDeviceInfo()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n📡 Device Info from camera: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine($"Body: {body}\n");

            _logger.LogDebug("Received device info from camera: {body}", body);

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing device info");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives quick plate result (if camera has this feature)
    /// Camera sends POST to: /devicemanagement/php/quickplateresult.php
    /// </summary>
    [HttpPost("quickplateresult.php")]
    public async Task<IActionResult> ReceiveQuickPlateResult()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("\n⚡ QUICK Plate Result:");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine($"Source: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Data: {body}\n");

            if (!string.IsNullOrWhiteSpace(body))
            {
                object plateData;
                try
                {
                    plateData = JsonSerializer.Deserialize<JsonElement>(body);
                }
                catch
                {
                    plateData = new { rawData = body, timestamp = DateTime.UtcNow };
                }

                await _plateDataStreamService.PublishPlateAsync(plateData);
            }

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quick plate result");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives GPIO trigger messages
    /// Camera sends POST to: /devicemanagement/php/gio.php
    /// </summary>
    [HttpPost("gio.php")]
    public async Task<IActionResult> ReceiveGpioTrigger()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n🔌 GPIO Trigger: {body}");
            _logger.LogDebug("Received GPIO trigger: {body}", body);

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GPIO trigger");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives port trigger messages
    /// Camera sends POST to: /devicemanagement/php/porttrigger.php (if configured)
    /// </summary>
    [HttpPost("porttrigger.php")]
    public async Task<IActionResult> ReceivePortTrigger()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n🔔 Port Trigger: {body}");
            _logger.LogDebug("Received port trigger: {body}", body);

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing port trigger");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives gate status
    /// Camera sends POST to: /devicemanagement/php/gate.php
    /// </summary>
    [HttpPost("gate.php")]
    public async Task<IActionResult> ReceiveGateStatus()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n🚧 Gate Status: {body}");
            _logger.LogDebug("Received gate status: {body}", body);

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing gate status");
            return Ok(new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Receives serial port data
    /// Camera sends POST to: /devicemanagement/php/serial.php
    /// </summary>
    [HttpPost("serial.php")]
    public async Task<IActionResult> ReceiveSerialData()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n📟 Serial Data: {body}");
            _logger.LogDebug("Received serial data: {body}", body);

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing serial data");
            return Ok(new { status = "error", message = ex.Message });
        }
    }
}