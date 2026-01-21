using System.Text.Json;
using Qalitrack.Models;

namespace Qalitrack.Services;

public class RuntimeConfigurationManager
{
    private readonly ILogger<RuntimeConfigurationManager> _logger;
    private readonly string _configFilePath;
    private readonly object _lock = new();
    private FileSystemWatcher? _fileWatcher;
    private DateTime _lastReloadTime = DateTime.MinValue;
    private readonly TimeSpan _reloadDebounce = TimeSpan.FromSeconds(2);

    public event Action<RuntimeConfiguration>? ConfigurationChanged;

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
        if (!string.IsNullOrEmpty(directory))
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Created config directory: {directory}", directory);
                }

                // Set directory permissions to allow writes (Linux/Unix)
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    try
                    {
                        var chmodProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = $"775 \"{directory}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                        chmodProcess?.WaitForExit();
                    }
                    catch
                    {
                        // Silently ignore if chmod fails
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create or set permissions on config directory: {directory}", directory);
            }
        }

        _logger.LogInformation("Config file path: {path}", _configFilePath);
        SetupFileWatcher();
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configFilePath);
            var filename = Path.GetFileName(_configFilePath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(filename))
                return;

            _fileWatcher = new FileSystemWatcher(directory)
            {
                Filter = filename,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnConfigFileChanged;
            _logger.LogInformation("File watcher configured for: {path}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not setup file watcher for config changes");
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file changes
        var now = DateTime.UtcNow;
        if (now - _lastReloadTime < _reloadDebounce)
            return;

        _lastReloadTime = now;

        try
        {
            // Wait a bit for file write to complete
            Thread.Sleep(100);

            _logger.LogInformation("Configuration file changed, reloading...");
            var defaultConfig = new RuntimeConfiguration();
            var newConfig = LoadConfiguration(defaultConfig);
            
            ConfigurationChanged?.Invoke(newConfig);
            _logger.LogInformation("Configuration reloaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration after file change");
        }
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
            // Try user-writable location first for services
            var userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "qalitrack", "qalitrack-config.json"
            );

            // Fallback to /var/lib which is typically writable by services
            var varLibPath = "/var/lib/qalitrack/qalitrack-config.json";
            
            // Check if we can write to /var/lib
            try
            {
                var varLibDir = "/var/lib/qalitrack";
                if (Directory.Exists(varLibDir) || CanCreateDirectory(varLibDir))
                {
                    return varLibPath;
                }
            }
            catch
            {
                // Fall through to user path
            }

            return userPath;
        }
        else if (OperatingSystem.IsWindows())
        {
            // Use LocalApplicationData which is writable by LocalSystem
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                localAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
            return Path.Combine(localAppData, "Qalitrack", "qalitrack-config.json");
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
            if (Directory.Exists(path))
                return true;

            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
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
                    // Retry logic for file locks
                    for (int i = 0; i < 3; i++)
                    {
                        try
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
                        catch (IOException) when (i < 2)
                        {
                            // File might be locked, wait and retry
                            Thread.Sleep(100);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("External config not found at {path}, creating default", _configFilePath);
                    
                    // Try to create default config, but don't fail if we can't
                    try
                    {
                        SaveConfiguration(defaultConfig);
                        _logger.LogInformation("Default configuration file created at: {path}", _configFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not create default config file, using in-memory defaults");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied accessing {path}. Check permissions or set QALITRACK_CONFIG_PATH", _configFilePath);
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

                // Temporarily disable file watcher to avoid triggering reload
                if (_fileWatcher != null)
                {
                    _fileWatcher.EnableRaisingEvents = false;
                }

                // Use atomic write pattern: write to temp file, then rename
                var tempFile = _configFilePath + ".tmp";
                var backupFile = _configFilePath + ".bak";

                try
                {
                    // Write to temporary file
                    File.WriteAllText(tempFile, json);

                    // Create backup if original exists
                    if (File.Exists(_configFilePath))
                    {
                        File.Copy(_configFilePath, backupFile, true);
                    }

                    // Atomic replace
                    File.Move(tempFile, _configFilePath, true);

                    _logger.LogInformation("Configuration saved to: {path}", _configFilePath);

                    // Set file permissions (Linux/Unix)
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        try
                        {
                            var chmodProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"664 \"{_configFilePath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            });
                            chmodProcess?.WaitForExit();
                        }
                        catch
                        {
                            // Silently ignore if chmod fails
                        }
                    }
                }
                finally
                {
                    // Cleanup temp file if it still exists
                    if (File.Exists(tempFile))
                    {
                        try { File.Delete(tempFile); } catch { }
                    }

                    // Re-enable file watcher after a short delay
                    if (_fileWatcher != null)
                    {
                        Task.Delay(500).ContinueWith(_ =>
                        {
                            _fileWatcher.EnableRaisingEvents = true;
                        });
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied writing to {path}. Run as administrator or check permissions.", _configFilePath);
                _logger.LogError("Suggested fix for Linux: sudo chown -R $USER:$USER {dir}", Path.GetDirectoryName(_configFilePath));
                _logger.LogError("Suggested fix for Windows: Run service as LocalSystem or grant write permissions");
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

    public void Dispose()
    {
        _fileWatcher?.Dispose();
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