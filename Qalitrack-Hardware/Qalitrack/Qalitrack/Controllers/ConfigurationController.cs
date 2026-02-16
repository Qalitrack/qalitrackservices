using Microsoft.AspNetCore.Mvc;
using Qalitrack.Services;
using Qalitrack.Models;

namespace Qalitrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly RuntimeConfigurationManager _configManager;
    private readonly IHostApplicationLifetime _lifetime;

    public ConfigurationController(
        ILogger<ConfigurationController> logger,
        RuntimeConfigurationManager configManager,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _configManager = configManager;
        _lifetime = lifetime;
    }

    /// <summary>
    /// Get current configuration
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfiguration()
    {
        try
        {
            var config = _configManager.LoadConfiguration(new RuntimeConfiguration());
            
            return Ok(new
            {
                configFilePath = _configManager.GetConfigFilePath(),
                configuration = new
                {
                    tcpListener = config.TcpListener != null ? new
                    {
                        ipAddress = config.TcpListener.IpAddress,
                        port = config.TcpListener.Port,
                        readTimeoutMs = config.TcpListener.ReadTimeoutMs,
                        reconnectDelayMs = config.TcpListener.ReconnectDelayMs,
                        serialPort = config.TcpListener.SerialPort,
                        baudRate = config.TcpListener.BaudRate
                    } : null,
                    cameraSettings = config.CameraSettings != null ? new
                    {
                        cameras = config.CameraSettings.Cameras?.Select(c => new
                        {
                            id = c.Id,
                            name = c.Name,
                            ipAddress = c.IpAddress,
                            port = c.Port,
                            username = c.Username,
                            password = "********", // Don't expose password
                            rtspPath = c.RtspPath,
                            enabled = c.Enabled,
                            supportsSnapshot = c.SupportsSnapshot,
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
                        }) ?? Enumerable.Empty<object>(),
                        reconnectDelayMs = config.CameraSettings.ReconnectDelayMs,
                        frameBufferSize = config.CameraSettings.FrameBufferSize
                    } : null,
                    rfidSettings = config.RfidSettings != null ? new
                    {
                        enabled = config.RfidSettings.Enabled,
                        host = config.RfidSettings.Host,
                        port = config.RfidSettings.Port,
                        scanIntervalMs = config.RfidSettings.ScanIntervalMs,
                        reconnectDelayMs = config.RfidSettings.ReconnectDelayMs
                    } : null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration");
            return StatusCode(500, new { error = "Failed to load configuration", message = ex.Message });
        }
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
            _logger.LogInformation("Saving new configuration...");
            _logger.LogDebug("New config: {Config}", System.Text.Json.JsonSerializer.Serialize(newConfig));
            
            var configPath = _configManager.GetConfigFilePath();
            _logger.LogInformation("Saving to config file: {ConfigPath}", configPath);
            
            _configManager.SaveConfiguration(newConfig);
            _logger.LogInformation("Configuration updated successfully");
            
            // Verify the file was written
            if (!System.IO.File.Exists(configPath))
            {
                _logger.LogError("Configuration file was not created at: {ConfigPath}", configPath);
                return StatusCode(500, new { error = "Configuration file was not created" });
            }
            
            _logger.LogInformation("Configuration file exists. Size: {Size} bytes", new FileInfo(configPath).Length);

            return Ok(new
            {
                message = "Configuration updated successfully",
                configFilePath = configPath,
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
            exists = System.IO.File.Exists(_configManager.GetConfigFilePath()),
            lastModified = _configManager.GetLastModifiedTime()
        });
    }
}