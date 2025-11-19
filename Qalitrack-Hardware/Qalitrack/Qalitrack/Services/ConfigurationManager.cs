using System.Text.Json;
using Qalitrack.Models;

namespace Qalitrack.Services;

public class RuntimeConfigurationManager
{
    private readonly ILogger<RuntimeConfigurationManager> _logger;
    private readonly string _configFilePath;
    private readonly object _lock = new();

    public RuntimeConfigurationManager(ILogger<RuntimeConfigurationManager> logger, string? configPath = null)
    {
        _logger = logger;

        // External config file - NOT compiled with the app
        _configFilePath = configPath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "qalitrack-config.json"
        );
    }

    public T LoadConfiguration<T>(T defaultConfig) where T : class
    {
        lock (_lock)
        {
            try
            {
                // Try to load from external config file first
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (config != null)
                    {
                        _logger.LogInformation("Configuration loaded from: {path}", _configFilePath);
                        return config;
                    }
                }
                else
                {
                    _logger.LogWarning("External config not found at {path}, using defaults", _configFilePath);

                    // Create default config file for easy editing
                    SaveConfiguration(defaultConfig);
                    _logger.LogInformation("Default configuration file created at: {path}", _configFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration from {path}, using defaults", _configFilePath);
            }

            return defaultConfig;
        }
    }

    public void SaveConfiguration<T>(T config) where T : class
    {
        lock (_lock)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_configFilePath, json);
                _logger.LogInformation("Configuration saved to: {path}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration to {path}", _configFilePath);
                throw;
            }
        }
    }

    public string GetConfigFilePath() => _configFilePath;
}

public class RuntimeConfiguration
{
    public ConnectionSettings TcpListener { get; set; } = new();
    public CameraSettings CameraSettings { get; set; } = new();
}
