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

    private enum ConnectionType { None, Tcp, Serial }
    private ConnectionType _activeConnectionType = ConnectionType.None;

    public PlatformDataService(
        ILogger<PlatformDataService> logger,
        IOptions<ConnectionSettings> settings,
        DataStreamService dataStreamService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _dataStreamService = dataStreamService ?? throw new ArgumentNullException(nameof(dataStreamService));

        _logger.LogInformation("PlatformDataService initializing — ConnectionType: {type}, {desc}",
            _settings.ConnectionType, _settings.GetConnectionDescription());
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
                _ = ReconnectAsync(CancellationToken.None);
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
        _logger.LogInformation("Starting PlatformDataService — mode: {type}", _settings.ConnectionType);

        // Resolve configured mode once — honour it exclusively, never fall back
        var configuredType = _settings.ConnectionType?.ToUpperInvariant() switch
        {
            "TCP"    => ConnectionType.Tcp,
            "SERIAL" => ConnectionType.Serial,
            _        => ConnectionType.Tcp   // unknown value → default to TCP and warn
        };

        if (configuredType == ConnectionType.Tcp &&
            !string.Equals(_settings.ConnectionType, "TCP", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Unrecognised ConnectionType '{type}' — defaulting to TCP. " +
                "Valid values are 'TCP' or 'Serial'.", _settings.ConnectionType);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool connected = false;

                if (configuredType == ConnectionType.Tcp)
                {
                    _logger.LogInformation("Connecting via TCP to {ip}:{port}",
                        _settings.IpAddress, _settings.Port);
                    connected = await TryConnectTcpAsync(stoppingToken);
                    if (connected) _activeConnectionType = ConnectionType.Tcp;
                }
                else // Serial
                {
                    _logger.LogInformation("Connecting via Serial to {port}", _settings.SerialPort);
                    connected = await TryConnectSerialAsync(stoppingToken);
                    if (connected) _activeConnectionType = ConnectionType.Serial;
                }

                if (connected)
                {
                    _logger.LogInformation("✓ {type} connection established and active",
                        _activeConnectionType);

                    await ProcessDataAsync(stoppingToken);

                    _logger.LogWarning("{type} connection lost — reconnecting in {delay}ms",
                        _activeConnectionType, _settings.ReconnectDelayMs);

                    lock (_connectionLock)
                        _activeConnectionType = ConnectionType.None;

                    await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
                }
                else
                {
                    _logger.LogWarning(
                        "{type} connection failed — retrying in {delay}ms",
                        configuredType, _settings.ReconnectDelayMs);
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
                    _activeConnectionType = ConnectionType.None;
                await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
            }
        }

        CleanupTcp();
        CleanupSerial();
        _logger.LogInformation("PlatformDataService stopped");
    }

    private async Task ProcessDataAsync(CancellationToken token)
    {
        if (_activeConnectionType == ConnectionType.Tcp && _tcpClient?.Connected == true)
            await ProcessTcpStreamAsync(token);
        else if (_activeConnectionType == ConnectionType.Serial && _serialPort?.IsOpen == true)
            await ProcessSerialStreamAsync(token);
    }

    private async Task<bool> TryConnectTcpAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_settings.IpAddress))
        {
            _logger.LogError("TCP connection configured but IpAddress is empty — check config");
            return false;
        }

        try
        {
            var tcpClient = new TcpClient
            {
                ReceiveTimeout = 30000,  // 30 seconds for scale data
                SendTimeout    = 30000
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                await tcpClient.ConnectAsync(_settings.IpAddress, _settings.Port, cts.Token);

                // Enable TCP keepalive to prevent idle connection drops
                tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // Configure keepalive parameters (2 min idle, 30s interval, 5 retries = ~4.5 min total)
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Windows: time (ms), interval (ms)
                    var keepAliveValues = new byte[12];
                    BitConverter.GetBytes(1).CopyTo(keepAliveValues, 0);        // on/off
                    BitConverter.GetBytes(120000).CopyTo(keepAliveValues, 4);   // time: 2 minutes
                    BitConverter.GetBytes(30000).CopyTo(keepAliveValues, 8);    // interval: 30 seconds
                    tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
                }
                else
                {
                    // Linux: time (seconds), interval (seconds), count
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 30);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
                }

                lock (_connectionLock)
                {
                    _tcpClient     = tcpClient;
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 30000;  // 30 seconds
                }

                _logger.LogInformation("✓ TCP connected to {ip}:{port} with keepalive enabled",
                    _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !token.IsCancellationRequested)
            {
                _logger.LogWarning("TCP connection to {ip}:{port} timed out after 10s",
                    _settings.IpAddress, _settings.Port);
                tcpClient.Dispose();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("TCP connection failed to {ip}:{port} — {message}",
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

            if (string.IsNullOrWhiteSpace(portName))
            {
                _logger.LogError("Serial connection configured but SerialPort is empty — check config");
                return false;
            }

            // Handle AUTO detection
            if (string.Equals(portName, "AUTO", StringComparison.OrdinalIgnoreCase))
            {
                portName = DetectSerialPort();
                if (portName == null)
                {
                    _logger.LogWarning("Serial AUTO detection found no available ports");
                    return false;
                }
            }
            // Normalise Windows COM port format for COM10+
            else if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
                     !portName.StartsWith("\\\\.\\", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(portName.Substring(3), out int comNumber) && comNumber >= 10)
                    portName = $"\\\\.\\{portName.ToUpper()}";
                else
                    portName = portName.ToUpper();
            }

            _logger.LogDebug("Opening serial port {port} at {baud} baud", portName, _settings.BaudRate);

            _serialPort = new SerialPort(
                portName, _settings.BaudRate, _settings.Parity, _settings.DataBits, _settings.StopBits)
            {
                ReadTimeout  = 30000,  // 30 seconds for scale data
                WriteTimeout = 30000,
                Encoding     = Encoding.ASCII,
                NewLine      = "\n"
            };

            _serialPort.Open();
            _logger.LogInformation("✓ Serial port connected to {port}", portName);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            var hint = isLinux
                ? "Run as root or add user to 'dialout' group."
                : "Port may be in use or service lacks Administrator rights.";
            _logger.LogWarning("Access denied to serial port {port}. {hint}", _settings.SerialPort, hint);
        }
        catch (IOException ex)
        {
            _logger.LogWarning("Serial port {port} not found or unavailable: {message}",
                _settings.SerialPort, ex.Message);
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
            string? preferred;

            if (isWindows)
            {
                preferred = ports.FirstOrDefault(p =>
                    p.Equals("COM9", StringComparison.OrdinalIgnoreCase))
                    ?? ports.FirstOrDefault(p =>
                    p.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                    ?? ports[0];
            }
            else
            {
                preferred = ports.FirstOrDefault(p =>
                    p.Contains("ttyUSB", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("ttyACM", StringComparison.OrdinalIgnoreCase))
                    ?? ports[0];
            }

            _logger.LogDebug("Auto-selected serial port: {port}", preferred);
            return preferred;
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
                ProcessIncomingData(Encoding.ASCII.GetString(buffer, 0, bytesRead), "TCP");
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
                    ProcessIncomingData(_serialPort.ReadExisting(), "Serial");
                else
                    await Task.Delay(10, token);
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
        _dataBuffer.Append(rawData);
        var bufferContent = _dataBuffer.ToString();
        var lines = bufferContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        bool endsWithNewline = bufferContent.EndsWith('\n') || bufferContent.EndsWith('\r');
        int linesToProcess = endsWithNewline ? lines.Length : lines.Length - 1;

        for (int i = 0; i < linesToProcess; i++)
        {
            var line = lines[i].Trim();
            if (!string.IsNullOrWhiteSpace(line))
                ProcessPlatformLine(line, source);
        }

        if (endsWithNewline)
            _dataBuffer.Clear();
        else if (lines.Length > 0)
        {
            _dataBuffer.Clear();
            _dataBuffer.Append(lines[^1]);
        }

        if (_dataBuffer.Length > 4096)
        {
            _logger.LogWarning("Buffer overflow, clearing buffer");
            _dataBuffer.Clear();
        }
    }

    private void ProcessPlatformLine(string line, string source)
    {
        var platformMatch = Regex.Match(line, @"Platform\s+(\d+)\s*:\s*(.+)", RegexOptions.IgnoreCase);
        var totalMatch    = Regex.Match(line, @"Total\s*:\s*(.+)",            RegexOptions.IgnoreCase);

        string type = "", platformNumber = "", weight = "";

        if (platformMatch.Success)
        {
            type           = "platform";
            platformNumber = platformMatch.Groups[1].Value;
            weight         = platformMatch.Groups[2].Value.Trim();
        }
        else if (totalMatch.Success)
        {
            type   = "total";
            weight = totalMatch.Groups[1].Value.Trim();
        }
        else
        {
            _logger.LogInformation("[{source}] {line}", source, line);
            _ = PublishData(line, source, "unknown", "", line);
            return;
        }

        var displayLine = type == "platform"
            ? $"Platform {platformNumber}: {weight}"
            : $"Total: {weight}";

        _logger.LogInformation("[{source}] {displayLine}", source, displayLine);
        _ = PublishData(line, source, type, platformNumber, weight);
    }

    private async Task PublishData(string rawLine, string source, string type,
        string platformNumber, string weight)
    {
        try
        {
            await _dataStreamService.PublishAsync(new
            {
                raw            = rawLine,
                type           = type,
                platformNumber = platformNumber,
                weight         = weight,
                source         = source,
                timestamp      = DateTime.UtcNow
            }, CancellationToken.None);
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
        catch (Exception ex) { _logger.LogError(ex, "Error cleaning up TCP resources"); }
        finally
        {
            _networkStream = null;
            _tcpClient     = null;
        }
    }

    private void CleanupSerial()
    {
        try
        {
            if (_serialPort?.IsOpen == true) _serialPort.Close();
            _serialPort?.Dispose();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Error during Serial cleanup"); }
        finally { _serialPort = null; }
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