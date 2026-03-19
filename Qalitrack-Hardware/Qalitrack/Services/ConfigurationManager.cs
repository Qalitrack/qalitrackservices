using System.Text.Json;
using System.Text.Json.Serialization;
using Qalitrack.Models;
using Microsoft.Extensions.Logging;

namespace Qalitrack.Services;

public class RuntimeConfigurationManager
{
    private readonly ILogger<RuntimeConfigurationManager> _logger;
    private readonly string _configFilePath;
    private readonly object _lock = new();
    private FileSystemWatcher? _fileWatcher;
    private DateTime _lastReloadTime = DateTime.MinValue;
    private readonly TimeSpan _reloadDebounce = TimeSpan.FromSeconds(2);

    // Shared options used for both load and save — enum names as strings, never integers
    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
        AllowTrailingCommas         = true,
        Converters                  = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
        Converters    = { new JsonStringEnumConverter() }
    };

    public event Action<RuntimeConfiguration>? ConfigurationChanged;

    public RuntimeConfigurationManager(ILogger<RuntimeConfigurationManager> logger, string? configPath = null)
    {
        _logger = logger;

        _configFilePath = !string.IsNullOrEmpty(configPath) ? configPath : GetDefaultConfigPath();

        EnsureConfigDirectoryExists();
        SetupFileWatcher();

        _logger.LogInformation("Config file path: {path}", _configFilePath);
    }

    /// <summary>
    /// Returns the canonical system-wide config path for the current OS.
    /// Priority: QALITRACK_CONFIG_PATH env var → OS-specific fixed path.
    ///
    /// Windows: C:\ProgramData\Qalitrack\qalitrack.json
    ///   - ProgramData is always accessible to LocalSystem and Administrators
    ///   - Never varies based on which user account runs the service
    ///
    /// Linux: /etc/qalitrack/qalitrack.json
    ///   - Standard location for system service configuration
    ///   - Consistent regardless of which user runs the service (root, dedicated user, etc.)
    ///   - Install script must create this directory and set appropriate permissions
    /// </summary>
    private static string GetDefaultConfigPath()
    {
        var envPath = Environment.GetEnvironmentVariable("QALITRACK_CONFIG_PATH");
        if (!string.IsNullOrEmpty(envPath))
            return envPath;

        if (OperatingSystem.IsWindows())
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(programData, "Qalitrack", "qalitrack.json");
        }

        if (OperatingSystem.IsLinux())
            return "/etc/qalitrack/qalitrack.json";

        if (OperatingSystem.IsMacOS())
        {
            var appSupport = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appSupport, "Qalitrack", "qalitrack.json");
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".qalitrack", "qalitrack.json");
    }

    private void EnsureConfigDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_configFilePath);
        if (string.IsNullOrEmpty(directory))
            return;

        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created config directory: {directory}", directory);

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    TryChmod("775", directory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex,
                "Cannot create config directory: {directory}. " +
                "On Linux run: sudo mkdir -p {directory} && sudo chown $(whoami) {directory}. " +
                "On Windows ensure the service account has write access to ProgramData\\Qalitrack.",
                directory, directory, directory);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create config directory: {directory}", directory);
        }
    }

    private void SetupFileWatcher()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configFilePath);
            var filename  = Path.GetFileName(_configFilePath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(filename) || !Directory.Exists(directory))
                return;

            _fileWatcher = new FileSystemWatcher(directory)
            {
                Filter            = filename,
                NotifyFilter      = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnConfigFileChanged;
            _logger.LogInformation("File watcher configured for: {path}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set up file watcher — hot-reload disabled");
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.UtcNow;
        if (now - _lastReloadTime < _reloadDebounce)
            return;

        _lastReloadTime = now;

        try
        {
            Thread.Sleep(150);
            _logger.LogInformation("Configuration file changed, reloading...");
            var newConfig = LoadConfiguration(new RuntimeConfiguration());
            ConfigurationChanged?.Invoke(newConfig);
            _logger.LogInformation("Configuration reloaded successfully from: {path}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration after file change");
        }
    }

    public T LoadConfiguration<T>(T defaultConfig) where T : class
    {
        lock (_lock)
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogWarning("Config file not found at {path} — creating default", _configFilePath);

                try
                {
                    SaveConfiguration(defaultConfig);
                    _logger.LogInformation("Default configuration file created at: {path}", _configFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not write default config — running with in-memory defaults");
                }

                return defaultConfig;
            }

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var json   = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<T>(json, _readOptions); // ← uses shared options with enum converter

                    if (config != null)
                    {
                        _logger.LogInformation("Configuration loaded from: {path}", _configFilePath);
                        return config;
                    }

                    _logger.LogWarning("Config file at {path} deserialized to null — using defaults", _configFilePath);
                    return defaultConfig;
                }
                catch (IOException) when (attempt < 3)
                {
                    _logger.LogDebug("Config file locked (attempt {attempt}/3), retrying...", attempt);
                    Thread.Sleep(100 * attempt);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex,
                        "Config file at {path} contains invalid JSON — using defaults. Fix the file and it will hot-reload.",
                        _configFilePath);
                    return defaultConfig;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex,
                        "Permission denied reading {path}. " +
                        "Linux fix: sudo chown $(whoami) {path}. " +
                        "Or set QALITRACK_CONFIG_PATH to a writable location.",
                        _configFilePath, _configFilePath);
                    return defaultConfig;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load config from {path} — using defaults", _configFilePath);
                    return defaultConfig;
                }
            }

            _logger.LogWarning("Config file locked after 3 attempts — using defaults");
            return defaultConfig;
        }
    }

    public void SaveConfiguration<T>(T config) where T : class
    {
        lock (_lock)
        {
            if (_fileWatcher != null)
                _fileWatcher.EnableRaisingEvents = false;

            var tempFile   = _configFilePath + ".tmp";
            var backupFile = _configFilePath + ".bak";

            try
            {
                var json = JsonSerializer.Serialize(config, _writeOptions); // ← uses shared options with enum converter

                File.WriteAllText(tempFile, json);

                if (File.Exists(_configFilePath))
                    File.Copy(_configFilePath, backupFile, overwrite: true);

                File.Move(tempFile, _configFilePath, overwrite: true);

                _logger.LogInformation("Configuration saved to: {path}", _configFilePath);

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    TryChmod("664", _configFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex,
                    "Permission denied writing to {path}. " +
                    "Linux: sudo chown $(whoami):$(whoami) {dir} && sudo chmod 775 {dir}. " +
                    "Windows: Grant the service account write access to {dir}.",
                    _configFilePath,
                    Path.GetDirectoryName(_configFilePath),
                    Path.GetDirectoryName(_configFilePath));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration to {path}", _configFilePath);
                throw;
            }
            finally
            {
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }

                if (_fileWatcher != null)
                {
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        try { _fileWatcher.EnableRaisingEvents = true; } catch { }
                    });
                }
            }
        }
    }

    private static void TryChmod(string mode, string target)
    {
        try
        {
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName            = "chmod",
                Arguments           = $"{mode} \"{target}\"",
                UseShellExecute     = false,
                CreateNoWindow      = true,
                RedirectStandardError = true
            });
            process?.WaitForExit(2000);
        }
        catch
        {
            // chmod failure is not fatal
        }
    }

    public bool ConfigFileExists()    => File.Exists(_configFilePath);
    public string GetConfigFilePath() => _configFilePath;

    public DateTime? GetLastModifiedTime()
    {
        try
        {
            return File.Exists(_configFilePath)
                ? File.GetLastWriteTimeUtc(_configFilePath)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get last modified time for {path}", _configFilePath);
            return null;
        }
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
        TcpListener    = new ConnectionSettings();
        CameraSettings = new CameraSettings();
        RfidSettings   = new RfidSettings();
        NfcSettings    = new NfcSettings();
    }

    public ConnectionSettings TcpListener    { get; set; }
    public CameraSettings     CameraSettings { get; set; }
    public RfidSettings       RfidSettings   { get; set; }
    public NfcSettings        NfcSettings    { get; set; }
}