using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/rfid")]
[EnableCors("AllowAll")]
public class RfidController : ControllerBase
{
    private readonly RfidTagStreamService _streamService;
    private readonly ILogger<RfidController> _logger;

    public RfidController(RfidTagStreamService streamService, ILogger<RfidController> logger)
    {
        _streamService = streamService;
        _logger = logger;
    }

    [HttpGet("stream")]
    [Produces("text/event-stream")]
    public async Task StreamTags(CancellationToken cancellationToken)
    {
        var clientId = $"sse_{Guid.NewGuid():N}";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogInformation("New RFID SSE client connected - ID: {ClientId} IP: {Ip}", clientId, ip);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        int count = 0;
        var sw = Stopwatch.StartNew();
        var lastActivity = DateTime.UtcNow;

        try
        {
            // Send initial connection confirmation
            await Response.WriteAsync($"data: {{\"type\":\"connected\",\"clientId\":\"{clientId}\"}}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // Create keepalive timer
            using var keepaliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            var keepaliveTask = Task.Run(async () =>
            {
                while (await keepaliveTimer.WaitForNextTickAsync(cancellationToken))
                {
                    var timeSinceLastActivity = DateTime.UtcNow - lastActivity;
                    if (timeSinceLastActivity.TotalSeconds >= 15)
                    {
                        try
                        {
                            await Response.WriteAsync(": keepalive\n\n", cancellationToken);
                            await Response.Body.FlushAsync(cancellationToken);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }, cancellationToken);

            await foreach (var json in _streamService.SubscribeAsync(clientId, cancellationToken))
            {
                count++;
                lastActivity = DateTime.UtcNow;
                
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RFID SSE client {ClientId} cancelled (normal disconnect)", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSE stream error for {ClientId}", clientId);
        }
        finally
        {
            _streamService.Unsubscribe(clientId);
            sw.Stop();
            _logger.LogInformation("RFID SSE client disconnected - ID: {ClientId} Messages: {Count} Duration: {Sec}s", 
                clientId, count, sw.Elapsed.TotalSeconds.ToString("F1"));
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "rfid", timestamp = DateTime.UtcNow });
    }
}