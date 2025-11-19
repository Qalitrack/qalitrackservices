using System;
using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CameraController : ControllerBase
{
    private readonly ILogger<CameraController> _logger;
    private readonly CameraStreamService _cameraStreamService;

    public CameraController(
        ILogger<CameraController> logger,
        CameraStreamService cameraStreamService)
    {
        _logger = logger;
        _cameraStreamService = cameraStreamService;
    }

    [HttpGet("{cameraId}/snapshot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSnapshot(string cameraId)
    {
        var cameras = _cameraStreamService.GetActiveCameras();
        var camera = cameras.FirstOrDefault(c => c.Id == cameraId);

        if (camera == null)
        {
            return NotFound($"Camera {cameraId} not found");
        }

        if (!camera.SupportsSnapshot)
        {
            return BadRequest($"Camera {cameraId} does not support snapshots. Use /stream endpoint instead.");
        }

        var frame = _cameraStreamService.GetLatestFrame(cameraId);

        if (frame.Length == 0)
        {
            return NotFound($"No frame available for camera {cameraId}");
        }

        return File(frame, "image/jpeg");
    }

    [HttpGet("{cameraId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus(string cameraId)
    {
        var status = _cameraStreamService.GetCameraStatus(cameraId);

        return Ok(status);
    }

    [HttpGet("cameras")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCameras()
    {
        var cameras = _cameraStreamService.GetActiveCameras();
        return Ok(cameras.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            hasNpr = c.NprSettings?.Enabled ?? false
        }));
    }

    [HttpGet("{cameraId}/stream")]
    [Produces("multipart/x-mixed-replace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task StreamCamera(string cameraId, CancellationToken cancellationToken)
    {
        var boundary = "frame";
        Response.ContentType = $"multipart/x-mixed-replace; boundary={boundary}";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        _logger.LogInformation("New camera stream connection for {cameraId} from {ip}", cameraId, HttpContext.Connection.RemoteIpAddress);

        try
        {
            await foreach (var frame in _cameraStreamService.StreamFramesAsync(cameraId, cancellationToken))
            {
                var header = $"--{boundary}\r\n" +
                           $"Content-Type: image/jpeg\r\n" +
                           $"Content-Length: {frame.Length}\r\n\r\n";

                await Response.WriteAsync(header, cancellationToken);
                await Response.Body.WriteAsync(frame, cancellationToken);
                await Response.WriteAsync("\r\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Camera {cameraId} stream connection closed", cameraId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming camera {cameraId}", cameraId);
        }
    }

}
