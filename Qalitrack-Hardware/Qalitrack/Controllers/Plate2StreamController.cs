using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/plates/lane2")]
[EnableCors("AllowAll")]
public class Plate2StreamController : ControllerBase
{
    private readonly PlateDataStreamService _plateDataStreamService;
    private readonly ILogger<Plate2StreamController> _logger;

    private static readonly object _lastPlateLock = new();
    private static (string Json, DateTime Time) _lastPlate = ("{}", DateTime.MinValue);

    private static (string Json, DateTime Time) LastPlate
    {
        get { lock (_lastPlateLock) return _lastPlate; }
        set { lock (_lastPlateLock) _lastPlate = value; }
    }

    public Plate2StreamController(
        [FromKeyedServices("lane2")] PlateDataStreamService plateDataStreamService,
        ILogger<Plate2StreamController> logger)
    {
        _plateDataStreamService = plateDataStreamService;
        _logger = logger;
    }

    [HttpGet("stream")]
    [Produces("text/event-stream")]
    public async Task StreamPlates(CancellationToken cancellationToken)
    {
        var clientId = $"sse_lane2_{Guid.NewGuid():N}";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        Console.WriteLine($"\n=== [Lane 2] New Client Connected ===");
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
            await foreach (var plateJson in _plateDataStreamService.SubscribeAsync(clientId, cancellationToken: cancellationToken))
            {
                messagesSent++;

                Console.WriteLine($"\n=== [Lane 2] New Plate Data (Client: {clientId}) ===");
                Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}");
                Console.WriteLine($"Raw JSON: {plateJson}");

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

                LastPlate = (plateJson, DateTime.UtcNow);

                await Response.WriteAsync($"data: {plateJson}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"[ERROR] [Lane 2] {DateTime.UtcNow:HH:mm:ss.fff} - Error in stream for {clientId}: {ex.Message}");
            _logger.LogError(ex, "[STREAM][Lane2] Error in stream for {ClientId}", clientId);
        }
        finally
        {
            _plateDataStreamService.Unsubscribe(clientId);
            sw.Stop();
            Console.WriteLine($"\n=== [Lane 2] Client Disconnected ===");
            Console.WriteLine($"Client ID: {clientId}");
            Console.WriteLine($"Messages Sent: {messagesSent}");
            Console.WriteLine($"Duration: {sw.Elapsed.TotalSeconds:F1}s");
            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss.fff}\n");
        }
    }
}
