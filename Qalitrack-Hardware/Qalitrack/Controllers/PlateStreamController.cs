using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/plates")]
[EnableCors("AllowAll")] // Add this if your frontend is on :3000, :5173, etc.
public class PlateStreamController : ControllerBase
{
    private readonly PlateDataStreamService _plateDataStreamService;
    private readonly ILogger<PlateStreamController> _logger;

    // Keep track of the most recent plate (for /latest endpoint)
    private static readonly object _lastPlateLock = new();
    private static (string Json, DateTime Time) _lastPlate = ("{}", DateTime.MinValue);
    
    private static (string Json, DateTime Time) LastPlate
    {
        get { lock (_lastPlateLock) return _lastPlate; }
        set { lock (_lastPlateLock) _lastPlate = value; }
    }

    public PlateStreamController(
        PlateDataStreamService plateDataStreamService,
        ILogger<PlateStreamController> logger)
    {
        _plateDataStreamService = plateDataStreamService;
        _logger = logger;
    }

    /// <summary>
    /// Stream all plate data from all cameras
    /// GET /api/plates/stream
    /// </summary>
    [HttpGet("stream")]
    [Produces("text/event-stream")]
    public async Task StreamPlates(CancellationToken cancellationToken)
    {
        await StreamPlatesInternal(null, cancellationToken);
    }

    /// <summary>
    /// Stream plate data from a specific camera
    /// GET /api/plates/stream/{cameraId}
    /// Examples:
    ///   - /api/plates/stream/front (front camera only)
    ///   - /api/plates/stream/back (back camera only)
    ///   - /api/plates/stream/cameraA (camera A only)
    ///   - /api/plates/stream/cameraB (camera B only)
    /// </summary>
    [HttpGet("stream/{cameraId}")]
    [Produces("text/event-stream")]
    public async Task StreamPlatesFromCamera(string cameraId, CancellationToken cancellationToken)
    {
        await StreamPlatesInternal(cameraId, cancellationToken);
    }

    private async Task StreamPlatesInternal(string? cameraId, CancellationToken cancellationToken)
    {
        var clientId = $"sse_{Guid.NewGuid():N}";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cameraInfo = cameraId != null ? $" (Camera: {cameraId})" : " (All Cameras)";

        Console.WriteLine($"\n=== New Client Connected{cameraInfo} ===");
        Console.WriteLine($"Client ID: {clientId}");
        Console.WriteLine($"IP: {ip}");
        Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        int messagesSent = 0;
        var sw = Stopwatch.StartNew();

        try
        {
            await foreach (var plateJson in _plateDataStreamService.SubscribeAsync(clientId, cameraId, cancellationToken))
            {
                messagesSent++;

                // Log the raw JSON to console
                Console.WriteLine($"\n=== New Plate Data{cameraInfo} (Client: {clientId}) ===");
                Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
                Console.WriteLine($"Raw JSON: {plateJson}");

                // Try to parse and pretty print the JSON
                try
                {
                    using var doc = JsonDocument.Parse(plateJson);
                    var formattedJson = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine("Formatted JSON:");
                    Console.WriteLine(formattedJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not format JSON: {ex.Message}");
                }

                // Update global latest plate
                LastPlate = (plateJson, DateTime.UtcNow);

                // Send to client
                await Response.WriteAsync($"data: {plateJson}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"[ERROR] {DateTime.UtcNow:HH:mm:ss.fff} - Error in stream for {clientId}: {ex.Message}");
            _logger.LogError(ex, "[STREAM] Error in stream for {ClientId}", clientId);
        }
        finally
        {
            _plateDataStreamService.Unsubscribe(clientId);
            sw.Stop();
            Console.WriteLine($"\n=== Client Disconnected{cameraInfo} ===");
            Console.WriteLine($"Client ID: {clientId}");
            Console.WriteLine($"Messages Sent: {messagesSent}");
            Console.WriteLine($"Duration: {sw.Elapsed.TotalSeconds:F1}s");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}\n");
        }
    }
}