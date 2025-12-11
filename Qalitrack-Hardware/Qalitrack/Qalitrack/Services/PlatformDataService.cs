using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Qalitrack.Models;

namespace Qalitrack.Services;

public class PlatformDataService : BackgroundService
{
    private readonly ILogger<PlatformDataService> _logger;
    private readonly ConnectionSettings _settings;
    private readonly DataStreamService _dataStreamService;

    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private SerialPort? _serialPort;

    private readonly object _connectionLock = new();
    private bool _disposed;
    private readonly StringBuilder _dataBuffer = new();
    
    // Track which connection type is active
    private ConnectionType _activeConnectionType = ConnectionType.None;
    
    private enum ConnectionType
    {
        None,
        Tcp,
        Serial
    }

    public PlatformDataService(
        ILogger<PlatformDataService> logger,
        IOptions<ConnectionSettings> settings,
        DataStreamService dataStreamService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _dataStreamService = dataStreamService ?? throw new ArgumentNullException(nameof(dataStreamService));

        _logger.LogInformation("PlatformDataService initializing with settings: {@Settings}", new
        {
            _settings.IpAddress,
            _settings.Port,
            _settings.SerialPort,
            _settings.BaudRate
        });
    }

    public bool IsConnected() => _tcpClient?.Connected == true || _serialPort?.IsOpen == true;

    public ConnectionSettings GetConnectionSettings() => _settings;

    public void ForceTcpSettings(string ip, int port)
    {
        if (string.IsNullOrWhiteSpace(ip))
            throw new ArgumentException("IP address cannot be empty", nameof(ip));
        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

        lock (_connectionLock)
        {
            _settings.IpAddress = ip;
            _settings.Port = port;
            _logger.LogInformation("Forced TCP settings to {ip}:{port}", ip, port);

            if (IsConnected())
            {
                _ = ReconnectAsync(CancellationToken.None);
            }
        }
    }

    private async Task ReconnectAsync(CancellationToken token)
    {
        lock (_connectionLock)
        {
            try
            {
                CleanupTcp();
                CleanupSerial();
                _activeConnectionType = ConnectionType.None;
                _logger.LogInformation("Attempting to reconnect to {ip}:{port}",
                    _settings.IpAddress, _settings.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup before reconnect");
            }
        }

        await TryConnectTcpAsync(token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting PlatformDataService background task");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool connected = false;

                // Try TCP first if configured and no active connection
                if (_activeConnectionType == ConnectionType.None && 
                    !string.IsNullOrWhiteSpace(_settings.IpAddress))
                {
                    _logger.LogInformation("Attempting TCP connection to {ip}:{port}",
                        _settings.IpAddress, _settings.Port);
                    connected = await TryConnectTcpAsync(stoppingToken);
                    
                    if (connected)
                    {
                        _activeConnectionType = ConnectionType.Tcp;
                        _logger.LogInformation("✓ TCP connection established and active");
                    }
                }

                // Try Serial only if TCP failed and no active connection
                if (_activeConnectionType == ConnectionType.None && 
                    !connected && 
                    !string.IsNullOrWhiteSpace(_settings.SerialPort))
                {
                    _logger.LogInformation("TCP unavailable, attempting serial connection to {port}",
                        _settings.SerialPort);
                    connected = await TryConnectSerialAsync(stoppingToken);
                    
                    if (connected)
                    {
                        _activeConnectionType = ConnectionType.Serial;
                        _logger.LogInformation("✓ Serial connection established and active");
                    }
                }

                if (connected)
                {
                    // Process data while connected
                    await ProcessDataAsync(stoppingToken);
                    
                    // Connection lost - log which one
                    var lostConnection = _activeConnectionType == ConnectionType.Tcp ? "TCP" : "Serial";
                    _logger.LogWarning("{connection} connection lost, will attempt to reconnect", lostConnection);
                    
                    // Reset active connection type
                    lock (_connectionLock)
                    {
                        _activeConnectionType = ConnectionType.None;
                    }
                    
                    // Wait before reconnecting
                    await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
                }
                else
                {
                    _logger.LogWarning("No connection established. Retrying in {delay}ms...",
                        _settings.ReconnectDelayMs);
                    await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Shutting down gracefully...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PlatformDataService background task");
                lock (_connectionLock)
                {
                    _activeConnectionType = ConnectionType.None;
                }
                await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
            }
        }

        CleanupTcp();
        CleanupSerial();
        _logger.LogInformation("PlatformDataService background task stopped");
    }

    private async Task ProcessDataAsync(CancellationToken token)
    {
        if (_activeConnectionType == ConnectionType.Tcp && _tcpClient?.Connected == true)
        {
            await ProcessTcpStreamAsync(token);
        }
        else if (_activeConnectionType == ConnectionType.Serial && _serialPort?.IsOpen == true)
        {
            await ProcessSerialStreamAsync(token);
        }
    }

    private async Task<bool> TryConnectTcpAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_settings.IpAddress))
        {
            _logger.LogDebug("TCP IP address is not configured, skipping TCP connection");
            return false;
        }

        try
        {
            _logger.LogDebug("Connecting to TCP {ip}:{port}...",
                _settings.IpAddress, _settings.Port);

            var tcpClient = new TcpClient
            {
                ReceiveTimeout = _settings.ReadTimeoutMs,
                SendTimeout = _settings.ReadTimeoutMs
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                await tcpClient.ConnectAsync(_settings.IpAddress, _settings.Port, cts.Token);

                lock (_connectionLock)
                {
                    _tcpClient = tcpClient;
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = _settings.ReadTimeoutMs;
                }

                _logger.LogInformation("✓ TCP connection established to {ip}:{port}",
                    _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !token.IsCancellationRequested)
            {
                _logger.LogWarning("TCP connection attempt to {ip}:{port} timed out after 10 seconds",
                    _settings.IpAddress, _settings.Port);
                tcpClient.Dispose();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "TCP connection failed to {ip}:{port}: {message}",
                    _settings.IpAddress, _settings.Port, ex.Message);
                tcpClient.Dispose();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TCP client for {ip}:{port}",
                _settings.IpAddress, _settings.Port);
            return false;
        }
    }

    private async Task<bool> TryConnectSerialAsync(CancellationToken token)
    {
        try
        {
            var portName = _settings.SerialPort;
            
            // Handle AUTO detection
            if (string.Equals(portName, "AUTO", StringComparison.OrdinalIgnoreCase))
            {
                portName = DetectSerialPort();
                if (portName == null)
                {
                    _logger.LogDebug("No serial ports detected");
                    return false;
                }
            }
            // Normalize Windows COM port format
            else if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) && 
                     !portName.StartsWith("\\\\.\\", StringComparison.OrdinalIgnoreCase))
            {
                // For COM ports >= 10, use the \\.\COMx format
                if (int.TryParse(portName.Substring(3), out int comNumber) && comNumber >= 10)
                {
                    portName = $"\\\\.\\{portName.ToUpper()}";
                    _logger.LogDebug("Using extended COM port format: {port}", portName);
                }
                else
                {
                    portName = portName.ToUpper();
                }
            }

            _logger.LogDebug("Connecting to Serial {port} at {baud} baud...",
                portName, _settings.BaudRate);

            _serialPort = new SerialPort(portName, _settings.BaudRate, _settings.Parity, _settings.DataBits, _settings.StopBits)
            {
                ReadTimeout = _settings.ReadTimeoutMs,
                WriteTimeout = _settings.ReadTimeoutMs,
                Encoding = Encoding.ASCII,
                NewLine = "\n"
            };

            _serialPort.Open();
            _logger.LogInformation("✓ Serial port connected successfully to {port}", portName);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            var suggestion = isLinux
                ? "Service should be running as root or user should be in 'dialout' group. Check systemd service configuration."
                : "Service should be running as LocalSystem/Administrator or COM port is already in use.";
            _logger.LogWarning("Access denied to {port}. {suggestion}", _settings.SerialPort, suggestion);
        }
        catch (IOException ex)
        {
            _logger.LogDebug("Serial port not found or unavailable: {message}", ex.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Serial connection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Serial connection failed: {message}", ex.Message);
        }
        finally
        {
            if (_serialPort?.IsOpen != true)
                CleanupSerial();
        }

        return false;
    }

    private string? DetectSerialPort()
    {
        try
        {
            var ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                _logger.LogDebug("No serial ports found on system");
                return null;
            }

            _logger.LogDebug("Available serial ports: {ports}", string.Join(", ", ports));

            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            
            string? preferredPort;
            if (isWindows)
            {
                // On Windows, prefer COM9 if available, otherwise first COM port
                preferredPort = ports.FirstOrDefault(p => 
                    p.Equals("COM9", StringComparison.OrdinalIgnoreCase)) 
                    ?? ports.FirstOrDefault(p => p.StartsWith("COM", StringComparison.OrdinalIgnoreCase)) 
                    ?? ports[0];
            }
            else
            {
                // On Linux, prefer ttyUSB or ttyACM
                preferredPort = ports.FirstOrDefault(p =>
                    p.Contains("ttyUSB", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("ttyACM", StringComparison.OrdinalIgnoreCase)) ?? ports[0];
            }

            _logger.LogDebug("Auto-selected serial port: {port}", preferredPort);
            return preferredPort;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting serial ports");
            return null;
        }
    }

    private async Task ProcessTcpStreamAsync(CancellationToken token)
    {
        var buffer = new byte[1024];

        while (!token.IsCancellationRequested && _tcpClient?.Connected == true)
        {
            try
            {
                var bytesRead = await _networkStream!.ReadAsync(buffer, token);
                if (bytesRead == 0)
                {
                    _logger.LogWarning("TCP connection closed by remote host");
                    break;
                }

                var rawData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                ProcessIncomingData(rawData, "TCP");
            }
            catch (OperationCanceledException) { break; }
            catch (IOException ex)
            {
                _logger.LogWarning("TCP stream error: {message}", ex.Message);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected TCP processing error");
                break;
            }
        }
    }

    private async Task ProcessSerialStreamAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _serialPort?.IsOpen == true)
        {
            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    var data = _serialPort.ReadExisting();
                    ProcessIncomingData(data, "Serial");
                }
                else
                {
                    await Task.Delay(10, token);
                }
            }
            catch (TimeoutException) { continue; }
            catch (OperationCanceledException) { break; }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Serial port closed: {message}", ex.Message);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Serial processing error");
                break;
            }
        }
    }

    private void ProcessIncomingData(string rawData, string source)
    {
        // Add incoming data to buffer
        _dataBuffer.Append(rawData);

        // Process complete lines (split by newline)
        var bufferContent = _dataBuffer.ToString();
        var lines = bufferContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Check if buffer ends with newline (complete line)
        bool endsWithNewline = bufferContent.EndsWith('\n') || bufferContent.EndsWith('\r');

        // Process all complete lines
        int linesToProcess = endsWithNewline ? lines.Length : lines.Length - 1;
        
        for (int i = 0; i < linesToProcess; i++)
        {
            var line = lines[i].Trim();
            if (!string.IsNullOrWhiteSpace(line))
            {
                ProcessPlatformLine(line, source);
            }
        }

        // Keep incomplete line in buffer
        if (endsWithNewline)
        {
            _dataBuffer.Clear();
        }
        else if (lines.Length > 0)
        {
            _dataBuffer.Clear();
            _dataBuffer.Append(lines[^1]);
        }

        // Prevent buffer from growing too large
        if (_dataBuffer.Length > 4096)
        {
            _logger.LogWarning("Buffer overflow, clearing buffer");
            _dataBuffer.Clear();
        }
    }

    private void ProcessPlatformLine(string line, string source)
    {
        // Parse platform readings: "Platform 1 :    -20 kg" or "Total      :     00 kg"
        var platformMatch = Regex.Match(line, @"Platform\s+(\d+)\s*:\s*(.+)", RegexOptions.IgnoreCase);
        var totalMatch = Regex.Match(line, @"Total\s*:\s*(.+)", RegexOptions.IgnoreCase);

        string type = "";
        string platformNumber = "";
        string weight = "";

        if (platformMatch.Success)
        {
            type = "platform";
            platformNumber = platformMatch.Groups[1].Value;
            weight = platformMatch.Groups[2].Value.Trim();
        }
        else if (totalMatch.Success)
        {
            type = "total";
            weight = totalMatch.Groups[1].Value.Trim();
        }
        else
        {
            // Unknown format, stream as-is
            _logger.LogInformation("[{source}] {line}", source, line);
            _ = PublishData(line, source, "unknown", "", line);
            return;
        }

        // Log formatted
        var displayLine = type == "platform" 
            ? $"Platform {platformNumber}: {weight}" 
            : $"Total: {weight}";
        
        _logger.LogInformation("[{source}] {displayLine}", source, displayLine);

        // Publish immediately
        _ = PublishData(line, source, type, platformNumber, weight);
    }

    private async Task PublishData(string rawLine, string source, string type, string platformNumber, string weight)
    {
        try
        {
            var payload = new
            {
                raw = rawLine,
                type = type,
                platformNumber = platformNumber,
                weight = weight,
                source = source,
                timestamp = DateTime.UtcNow
            };

            await _dataStreamService.PublishAsync(payload, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish data from {source}", source);
        }
    }

    private void CleanupTcp()
    {
        try
        {
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up TCP resources");
        }
        finally
        {
            _networkStream = null;
            _tcpClient = null;
        }
    }

    private void CleanupSerial()
    {
        try
        {
            if (_serialPort?.IsOpen == true)
                _serialPort.Close();
            _serialPort?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during Serial cleanup");
        }
        finally
        {
            _serialPort = null;
        }
    }

    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        CleanupTcp();
        CleanupSerial();
        base.Dispose();
    }
}