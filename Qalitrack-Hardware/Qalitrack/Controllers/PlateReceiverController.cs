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
        return await ProcessPlateResult(null);
    }

    /// <summary>
    /// Receives plate recognition results from a specific camera
    /// Camera sends POST to: /devicemanagement/php/plateresult/{cameraId}.php
    /// Examples:
    ///   - /devicemanagement/php/plateresult/front.php (front camera)
    ///   - /devicemanagement/php/plateresult/back.php (back camera)
    ///   - /devicemanagement/php/plateresult/cameraA.php (camera A)
    ///   - /devicemanagement/php/plateresult/cameraB.php (camera B)
    /// </summary>
    [HttpPost("plateresult/{cameraId}.php")]
    public async Task<IActionResult> ReceivePlateResultFromCamera(string cameraId)
    {
        return await ProcessPlateResult(cameraId);
    }

    private async Task<IActionResult> ProcessPlateResult(string? cameraId)
    {
        try
        {
            // Read the raw body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var cameraLabel = string.IsNullOrEmpty(cameraId) ? "UNKNOWN" : cameraId.ToUpper();

            // Log everything for debugging
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║          PLATE DATA RECEIVED FROM CAMERA: {cameraLabel,-15}║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Camera ID: {cameraId ?? "not specified"}");
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
                _logger.LogWarning("Received empty plate data from camera {cameraId}", cameraId);
                return Ok(new { status = "received", message = "empty_body", cameraId });
            }

            // Try to parse as JSON
            object plateData;
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);

                // Add camera ID to the data
                var dataWithCamera = new Dictionary<string, object>
                {
                    ["cameraId"] = cameraId ?? "unknown",
                    ["sourceIp"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    ["receivedAt"] = DateTime.UtcNow
                };

                // Copy all properties from the original JSON
                foreach (var property in jsonElement.EnumerateObject())
                {
                    dataWithCamera[property.Name] = property.Value;
                }

                plateData = dataWithCamera;

                // Pretty print the JSON
                var prettyJson = JsonSerializer.Serialize(plateData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine("--- Parsed JSON (with camera ID) ---");
                Console.WriteLine(prettyJson);
                Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
            }
            catch (JsonException)
            {
                // Not JSON, treat as plain text or form data
                _logger.LogInformation("Received non-JSON plate data from camera {cameraId}: {body}", cameraId, body);
                plateData = new
                {
                    cameraId = cameraId ?? "unknown",
                    sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    rawData = body,
                    receivedAt = DateTime.UtcNow
                };
            }

            // Publish to all connected SSE clients
            await _plateDataStreamService.PublishPlateAsync(plateData);

            var clientCount = _plateDataStreamService.GetClientCount();
            _logger.LogInformation("Plate data from camera {cameraId} published to {clientCount} clients", cameraId, clientCount);

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
                cameraId = cameraId ?? "unknown",
                received = DateTime.UtcNow,
                clientsNotified = clientCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing plate data from camera {cameraId}", cameraId);
            Console.WriteLine($"\n❌ ERROR processing plate data from camera {cameraId}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}\n");

            // Still return OK to camera so it doesn't retry
            return Ok(new { status = "error", message = ex.Message, cameraId });
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
        return await ProcessQuickPlateResult(null);
    }

    /// <summary>
    /// Receives quick plate result from a specific camera
    /// Camera sends POST to: /devicemanagement/php/quickplateresult/{cameraId}.php
    /// </summary>
    [HttpPost("quickplateresult/{cameraId}.php")]
    public async Task<IActionResult> ReceiveQuickPlateResultFromCamera(string cameraId)
    {
        return await ProcessQuickPlateResult(cameraId);
    }

    private async Task<IActionResult> ProcessQuickPlateResult(string? cameraId)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine($"\n⚡ QUICK Plate Result from Camera: {cameraId ?? "UNKNOWN"}");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine($"Source: {HttpContext.Connection.RemoteIpAddress}");
            Console.WriteLine($"Data: {body}\n");

            if (!string.IsNullOrWhiteSpace(body))
            {
                object plateData;
                try
                {
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);

                    // Add camera ID to the data
                    var dataWithCamera = new Dictionary<string, object>
                    {
                        ["cameraId"] = cameraId ?? "unknown",
                        ["sourceIp"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        ["receivedAt"] = DateTime.UtcNow,
                        ["quickResult"] = true
                    };

                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        dataWithCamera[property.Name] = property.Value;
                    }

                    plateData = dataWithCamera;
                }
                catch
                {
                    plateData = new
                    {
                        cameraId = cameraId ?? "unknown",
                        sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        rawData = body,
                        receivedAt = DateTime.UtcNow,
                        quickResult = true
                    };
                }

                await _plateDataStreamService.PublishPlateAsync(plateData);
            }

            return Ok(new { status = "success", cameraId = cameraId ?? "unknown" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quick plate result from camera {cameraId}", cameraId);
            return Ok(new { status = "error", message = ex.Message, cameraId });
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