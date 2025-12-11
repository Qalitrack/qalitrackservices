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

        if (!string.IsNullOrEmpty(configPath))
        {
            _configFilePath = configPath;
        }
        else
        {
            _configFilePath = GetDefaultConfigPath();
        }

        var directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created config directory: {directory}", directory);
        }

        _logger.LogInformation("Config file path: {path}", _configFilePath);
    }

    private static string GetDefaultConfigPath()
    {
        var envPath = Environment.GetEnvironmentVariable("QALITRACK_CONFIG_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            return envPath;
        }

        if (OperatingSystem.IsLinux())
        {
            var systemPath = "/etc/qalitrack/qalitrack-config.json";
            var userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "qalitrack", "qalitrack-config.json"
            );

            try
            {
                var testDir = "/etc/qalitrack";
                if (Directory.Exists(testDir) || CanCreateDirectory(testDir))
                {
                    return systemPath;
                }
            }
            catch
            {
            }

            return userPath;
        }
        else if (OperatingSystem.IsWindows())
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(programData, "Qalitrack", "qalitrack-config.json");
        }
        else if (OperatingSystem.IsMacOS())
        {
            var appSupport = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appSupport, "Qalitrack", "qalitrack-config.json");
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".qalitrack", "qalitrack-config.json");
        }
    }

    private static bool CanCreateDirectory(string path)
    {
        try
        {
            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent))
                return false;

            if (!Directory.Exists(parent))
                return false;

            var testFile = Path.Combine(parent, $".qalitrack_test_{Guid.NewGuid()}");
            try
            {
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public T LoadConfiguration<T>(T defaultConfig) where T : class
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    });

                    if (config != null)
                    {
                        _logger.LogInformation("Configuration loaded from: {path}", _configFilePath);
                        return config;
                    }
                }
                else
                {
                    _logger.LogWarning("External config not found at {path}, creating default", _configFilePath);
                    SaveConfiguration(defaultConfig);
                    _logger.LogInformation("Default configuration file created at: {path}", _configFilePath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied accessing {path}. Run as administrator or check permissions.", _configFilePath);
                _logger.LogWarning("Using in-memory default configuration");
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied writing to {path}. Run as administrator or check permissions.", _configFilePath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration to {path}", _configFilePath);
                throw;
            }
        }
    }

    public bool ConfigFileExists() => File.Exists(_configFilePath);

    public string GetConfigFilePath() => _configFilePath;

    public DateTime? GetLastModifiedTime()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                return File.GetLastWriteTimeUtc(_configFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get last modified time for {path}", _configFilePath);
        }
        return null;
    }
}
public class RuntimeConfiguration
{
    public RuntimeConfiguration()
    {
        TcpListener = new ConnectionSettings();
        CameraSettings = new CameraSettings();
    }
    
    public ConnectionSettings TcpListener { get; set; }
    public CameraSettings CameraSettings { get; set; }
}