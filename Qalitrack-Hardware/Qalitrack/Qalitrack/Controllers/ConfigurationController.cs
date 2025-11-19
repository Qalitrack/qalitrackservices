using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly RuntimeConfigurationManager _configManager;
    private readonly RuntimeConfiguration _runtimeConfig;
    private readonly IHostApplicationLifetime _lifetime;

    public ConfigurationController(
        ILogger<ConfigurationController> logger,
        RuntimeConfigurationManager configManager,
        RuntimeConfiguration runtimeConfig,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _configManager = configManager;
        _runtimeConfig = runtimeConfig;
        _lifetime = lifetime;
    }

    /// <summary>
    /// Get current configuration
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfiguration()
    {
        return Ok(new
        {
            configFilePath = _configManager.GetConfigFilePath(),
            configuration = new
            {
                tcpListener = new
                {
                    ipAddress = _runtimeConfig.TcpListener.TcpListener.IpAddress,
                    port = _runtimeConfig.TcpListener.TcpListener.Port,
                    readTimeoutMs = _runtimeConfig.TcpListener.TcpListener.ReadTimeoutMs,
                    reconnectDelayMs = _runtimeConfig.TcpListener.TcpListener.ReconnectDelayMs
                },
                cameraSettings = new
                {
                    cameras = _runtimeConfig.CameraSettings.Cameras.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        ipAddress = c.IpAddress,
                        port = c.Port,
                        username = c.Username,
                        password = "********", // Don't expose password
                        rtspPath = c.RtspPath,
                        enabled = c.Enabled,
                        nprSettings = c.NprSettings != null ? new
                        {
                            enabled = c.NprSettings.Enabled,
                            cameraQueryUrl = c.NprSettings.CameraQueryUrl,
                            webSocketUrl = c.NprSettings.WebSocketUrl,
                            username = c.NprSettings.Username,
                            password = "********", // Don't expose password
                            pollingIntervalMs = c.NprSettings.PollingIntervalMs,
                            useWebSocket = c.NprSettings.UseWebSocket
                        } : null
                    }),
                    reconnectDelayMs = _runtimeConfig.CameraSettings.ReconnectDelayMs,
                    frameBufferSize = _runtimeConfig.CameraSettings.FrameBufferSize
                }
            }
        });
    }

    /// <summary>
    /// Update configuration
    /// WARNING: Requires service restart to take effect
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateConfiguration([FromBody] RuntimeConfiguration newConfig)
    {
        try
        {
            _configManager.SaveConfiguration(newConfig);

            _logger.LogInformation("Configuration updated successfully");

            return Ok(new
            {
                message = "Configuration updated successfully",
                configFilePath = _configManager.GetConfigFilePath(),
                note = "Restart the service for changes to take effect",
                restartEndpoint = "/api/configuration/restart"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update configuration");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Restart the service to apply configuration changes
    /// </summary>
    [HttpPost("restart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RestartService()
    {
        _logger.LogWarning("Service restart requested via API");

        // Schedule restart
        Task.Run(async () =>
        {
            await Task.Delay(1000); // Give time to send response
            _lifetime.StopApplication();
        });

        return Ok(new
        {
            message = "Service restart initiated",
            note = "Service will restart in 1 second. Auto-restart is configured, so it will come back online automatically."
        });
    }

    /// <summary>
    /// Get configuration file path
    /// </summary>
    [HttpGet("path")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfigPath()
    {
        return Ok(new
        {
            configFilePath = _configManager.GetConfigFilePath(),
            exists = System.IO.File.Exists(_configManager.GetConfigFilePath())
        });
    }
}
