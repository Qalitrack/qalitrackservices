using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
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

    private readonly object _dataLock = new();
    private readonly object _connectionLock = new();
    private double[] _latestPlatformWeights = Array.Empty<double>();
    private bool _disposed;

    public double[] LatestPlatformWeights
    {
        get { lock (_dataLock) return (double[])_latestPlatformWeights.Clone(); }
        private set { lock (_dataLock) _latestPlatformWeights = value; }
    }

    private const int MaxPlatformCount = 16;
    private const int MaxBufferSize = 10000;
    private static readonly Regex PlatformWeightRegex = new(@"Platform\s+(\d+)\s*:\s*([\d.]+)\s*kg", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NumberRegex = new(@"-?\d+\.?\d*(?:[eE][+-]?\d+)?", RegexOptions.Compiled);

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
            _settings.BaudRate,
            _settings.ReadTimeoutMs,
            _settings.ReconnectDelayMs
        });

        _logger.LogInformation("Environment variables - " +
            "QALITRACK_TCP_IP: {TcpIp}, " +
            "QALITRACK_TCP_PORT: {TcpPort}",
            Environment.GetEnvironmentVariable("QALITRACK_TCP_IP") ?? "Not set",
            Environment.GetEnvironmentVariable("QALITRACK_TCP_PORT") ?? "Not set");
    }

    public bool IsConnected() => _tcpClient?.Connected == true;

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

                if (!string.IsNullOrWhiteSpace(_settings.IpAddress))
                {
                    _logger.LogDebug("Attempting TCP connection to {ip}:{port}",
                        _settings.IpAddress, _settings.Port);
                    connected = await TryConnectTcpAsync(stoppingToken);
                }

                if (!connected && !string.IsNullOrWhiteSpace(_settings.SerialPort))
                {
                    _logger.LogDebug("TCP connection failed, attempting serial connection");
                    connected = await TryConnectSerialAsync(stoppingToken);
                }

                if (connected)
                {
                    _logger.LogInformation("Connected to data source");
                    await ProcessDataAsync(stoppingToken);
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
                await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
            }
        }

        CleanupTcp();
        CleanupSerial();
        _logger.LogInformation("PlatformDataService background task stopped");
    }

    private async Task ProcessDataAsync(CancellationToken token)
    {
        if (_tcpClient?.Connected == true)
            await ProcessTcpStreamAsync(token);
        else if (_serialPort?.IsOpen == true)
            await ProcessSerialStreamAsync(token);
    }

    private async Task<bool> TryConnectTcpAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_settings.IpAddress))
        {
            _logger.LogWarning("TCP IP address is not configured");
            return false;
        }

        try
        {
            _logger.LogInformation("Attempting TCP connection to {ip}:{port}",
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

                _logger.LogInformation("TCP connection established to {ip}:{port}",
                    _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                _logger.LogWarning("TCP connection attempt to {ip}:{port} timed out after 10 seconds",
                    _settings.IpAddress, _settings.Port);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP connection failed to {ip}:{port}",
                    _settings.IpAddress, _settings.Port);
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
            if (string.Equals(portName, "AUTO", StringComparison.OrdinalIgnoreCase))
            {
                portName = DetectSerialPort();
                if (portName == null)
                {
                    _logger.LogWarning("No serial ports detected");
                    return false;
                }
            }

            _logger.LogInformation("Attempting Serial connection to {port} at {baud} baud",
                portName, _settings.BaudRate);

            _serialPort = new SerialPort(portName, _settings.BaudRate, _settings.Parity, _settings.DataBits, _settings.StopBits)
            {
                ReadTimeout = _settings.ReadTimeoutMs,
                WriteTimeout = _settings.ReadTimeoutMs,
                Encoding = Encoding.ASCII
            };

            _serialPort.Open();
            _logger.LogInformation("Serial port connected successfully");
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            var suggestion = isLinux
                ? "Service should be running as root. Check systemd service configuration."
                : "Service should be running as LocalSystem/Administrator.";
            _logger.LogError("Access denied to {port}. {suggestion}", _settings.SerialPort, suggestion);
        }
        catch (IOException ex)
        {
            _logger.LogWarning("Serial port not found or unavailable: {message}", ex.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Serial connection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Serial connection failed");
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
                _logger.LogWarning("No serial ports found on system");
                return null;
            }

            _logger.LogInformation("Available serial ports: {ports}", string.Join(", ", ports));

            var preferredPort = ports.FirstOrDefault(p =>
                p.Contains("ttyUSB", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("ttyACM", StringComparison.OrdinalIgnoreCase)) ?? ports[0];

            _logger.LogInformation("Auto-selected serial port: {port}", preferredPort);
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
        var messageBuffer = new StringBuilder();

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
                Console.WriteLine($"[TCP RAW] {rawData.Replace("\r", "\\r").Replace("\n", "\\n")} ({bytesRead} bytes)");
                _logger.LogInformation("TCP RAW RESPONSE: {data}", rawData.TrimEnd());

                messageBuffer.Append(rawData);

                if (rawData.Contains('\n') || rawData.Contains('\r'))
                {
                    var numbers = ExtractAllNumbers(messageBuffer.ToString());
                    if (numbers.Count > 0)
                    {
                        await HandleNumbers(numbers, "TCP");
                        messageBuffer.Clear();
                    }
                }

                if (messageBuffer.Length > MaxBufferSize)
                {
                    _logger.LogWarning("TCP buffer exceeded {max} bytes, clearing", MaxBufferSize);
                    messageBuffer.Clear();
                }
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
        var lineBuffer = new StringBuilder();

        while (!token.IsCancellationRequested && _serialPort?.IsOpen == true)
        {
            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    var data = _serialPort.ReadExisting();
                    Console.WriteLine($"[SERIAL RAW] {data.Replace("\r", "\\r").Replace("\n", "\\n")}");
                    _logger.LogInformation("SERIAL RAW RESPONSE: {data}", data.TrimEnd());

                    lineBuffer.Append(data);

                    if (data.Contains('\n') || data.Contains('\r'))
                    {
                        var numbers = ExtractAllNumbers(lineBuffer.ToString());
                        if (numbers.Count > 0)
                        {
                            await HandleNumbers(numbers, "Serial");
                            lineBuffer.Clear();
                        }
                    }

                    if (lineBuffer.Length > MaxBufferSize)
                    {
                        _logger.LogWarning("Serial buffer exceeded {max} bytes, clearing", MaxBufferSize);
                        lineBuffer.Clear();
                    }
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

    private List<double> ExtractAllNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<double>();

        var numbers = new List<double>();
        var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var matches = PlatformWeightRegex.Matches(line);
            foreach (Match match in matches)
            {
                if (double.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double weight))
                {
                    numbers.Add(weight);
                }
            }
        }

        if (numbers.Count == 0)
        {
            var matches = NumberRegex.Matches(input);
            foreach (Match match in matches)
            {
                if (double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    numbers.Add(value);
                }
            }
        }

        if (numbers.Count > 0)
        {
            _logger.LogDebug("Extracted {count} numbers: [{values}]",
                numbers.Count, string.Join(", ", numbers.Select(n => n.ToString("F2"))));
        }

        return numbers;
    }

    private async Task HandleNumbers(List<double> numbers, string source)
    {
        if (numbers.Count > MaxPlatformCount)
        {
            _logger.LogWarning("Truncated {excess} platform values (received {count} but max is {max})",
                numbers.Count - MaxPlatformCount, numbers.Count, MaxPlatformCount);
            numbers = numbers.Take(MaxPlatformCount).ToList();
        }

        var platformData = numbers.ToArray();
        LatestPlatformWeights = platformData;

        try
        {
            var payload = new { weight = platformData };
            await _dataStreamService.PublishAsync(payload, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast data from {source}", source);
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