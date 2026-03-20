using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Qalitrack.Models;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformDataController : ControllerBase
{
    private readonly ILogger<PlatformDataController> _logger;
    private readonly PlatformDataService _platformDataService;
    private readonly DataStreamService _dataStreamService;
    private readonly PlateDataStreamService _plateDataStreamService;

    public PlatformDataController(
        ILogger<PlatformDataController> logger,
        PlatformDataService platformDataService,
        DataStreamService dataStreamService,
        PlateDataStreamService plateDataStreamService)
    {
        _logger = logger;
        _platformDataService = platformDataService;
        _dataStreamService = dataStreamService;
        _plateDataStreamService = plateDataStreamService;
    }

    /// <summary>
    /// Streams platform data in real-time using Server-Sent Events (SSE)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the stream</param>
    /// <returns>Stream of platform data events</returns>
    [HttpGet("stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StreamPlatformData(CancellationToken cancellationToken)
    {
        var clientId = $"{Request.HttpContext.Connection.Id}:{Guid.NewGuid()}";
        _logger.LogInformation("New SSE connection from client {ClientId}", clientId);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Client-Id"] = clientId;

        var stream = _dataStreamService.SubscribeAsync(clientId, cancellationToken);
        var lastActivity = DateTime.UtcNow;
        System.Threading.Timer? heartbeatTimer = null;

        try
        {
            // Send initial connection message
            await Response.WriteAsync("event: connected\ndata: {\"status\":\"connected\"}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // Start heartbeat timer (every 30 seconds)
            heartbeatTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Response.WriteAsync(": heartbeat\n\n", cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    // Ignore if the connection was closed
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending heartbeat to client {ClientId}", clientId);
                    // Don't rethrow - this is a background operation
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            await foreach (var jsonData in stream.WithCancellation(cancellationToken))
            {
                try
                {
                    lastActivity = DateTime.UtcNow;
                    var message = $"data: {jsonData}\n\n";
                    await Response.WriteAsync(message, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    _logger.LogDebug("Client {ClientId} disconnected (operation canceled)", clientId);
                    break;
                }
                catch (IOException ex)
                {
                    _logger.LogDebug("Client {ClientId} disconnected (IO error): {Message}", clientId, ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending data to client {ClientId}", clientId);
                    // Don't break here - let the stream continue if possible
                }

                // Check for client timeout (5 minutes of inactivity)
                if ((DateTime.UtcNow - lastActivity) > TimeSpan.FromMinutes(5))
                {
                    _logger.LogInformation("Client {ClientId} timed out due to inactivity", clientId);
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error in platform data stream for client {ClientId}", clientId);
        }
        finally
        {
            try
            {
                heartbeatTimer?.Dispose();
                _dataStreamService.Unsubscribe(clientId);
                _logger.LogInformation("SSE connection closed for client {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup for client {ClientId}", clientId);
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Streams plate recognition data in real-time using Server-Sent Events (SSE)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the stream</param>
    [HttpGet("plates/stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StreamPlateData(CancellationToken cancellationToken)
    {
        var clientId = $"{Request.HttpContext.Connection.Id}:{Guid.NewGuid()}";
        _logger.LogInformation("New plate SSE connection from client {ClientId}", clientId);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Client-Id"] = clientId;

        var stream = _plateDataStreamService.SubscribeAsync(clientId, cancellationToken);
        var lastActivity = DateTime.UtcNow;
        System.Threading.Timer? heartbeatTimer = null;

        try
        {
            // Start heartbeat timer (every 30 seconds)
            heartbeatTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Response.WriteAsync(": heartbeat\n\n", cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    // Ignore if the connection was closed
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending heartbeat to client {ClientId}", clientId);
                    // Don't rethrow - this is a background operation
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            await foreach (var jsonData in stream.WithCancellation(cancellationToken))
            {
                try
                {
                    lastActivity = DateTime.UtcNow;
                    var message = $"data: {jsonData}\n\n";
                    await Response.WriteAsync(message, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    _logger.LogDebug("Client {ClientId} disconnected (operation canceled)", clientId);
                    break;
                }
                catch (IOException ex)
                {
                    _logger.LogDebug("Client {ClientId} disconnected (IO error): {Message}", clientId, ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending data to client {ClientId}", clientId);
                    // Don't break here - let the stream continue if possible
                }

                // Check for client timeout (5 minutes of inactivity)
                if ((DateTime.UtcNow - lastActivity) > TimeSpan.FromMinutes(5))
                {
                    _logger.LogInformation("Client {ClientId} timed out due to inactivity", clientId);
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error in plate data stream for client {ClientId}", clientId);
        }
        finally
        {
            try
            {
                heartbeatTimer?.Dispose();
                _plateDataStreamService.Unsubscribe(clientId);
                _logger.LogInformation("SSE connection closed for client {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup for client {ClientId}", clientId);
            }
        }

        return new EmptyResult();
    }
}