using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Qalitrack.Models;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Qalitrack.Services;

public class CameraStreamService : BackgroundService
{
    private readonly ILogger<CameraStreamService> _logger;
    private readonly CameraSettings _settings;
    private readonly CameraDataStreamService _cameraDataStreamService;

    private readonly ConcurrentDictionary<string, CameraStream> _cameraStreams = new();
    private string? _ffmpegPath;

    // Health monitoring configuration
    private const int FRAME_TIMEOUT_SECONDS = 10; // If no frame in 10 seconds, reconnect
    private const int HEALTH_CHECK_INTERVAL_MS = 2000; // Check health every 2 seconds

    public CameraStreamService(
        ILogger<CameraStreamService> logger,
        CameraSettings settings,
        CameraDataStreamService cameraDataStreamService)
    {
        _logger = logger;
        _settings = settings;
        _cameraDataStreamService = cameraDataStreamService;
    }

    public byte[] GetLatestFrame(string cameraId)
    {
        if (_cameraStreams.TryGetValue(cameraId, out var stream))
        {
            lock (stream.FrameLock)
            {
                return (byte[])stream.LatestFrame.Clone();
            }
        }
        return Array.Empty<byte>();
    }

    public List<Camera> GetActiveCameras()
    {
        return _settings.Cameras.Where(c => c.Enabled).ToList();
    }

    public CameraStreamStatus GetCameraStatus(string cameraId)
    {
        if (_cameraStreams.TryGetValue(cameraId, out var stream))
        {
            var timeSinceLastFrame = DateTime.UtcNow - stream.LastFrameTime;
            var isHealthy = stream.IsConnected && 
                           stream.LatestFrame.Length > 0 && 
                           timeSinceLastFrame.TotalSeconds < FRAME_TIMEOUT_SECONDS;

            return new CameraStreamStatus
            {
                CameraId = cameraId,
                IsConnected = stream.IsConnected,
                HasFrame = stream.LatestFrame.Length > 0,
                FrameSize = stream.LatestFrame.Length,
                LastFrameTime = stream.LastFrameTime,
                IsHealthy = isHealthy,
                SecondsSinceLastFrame = (int)timeSinceLastFrame.TotalSeconds
            };
        }
        return new CameraStreamStatus { CameraId = cameraId, IsConnected = false, IsHealthy = false };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CameraStreamService starting with {count} cameras", _settings.Cameras.Count);

        _ffmpegPath = FindFFmpegPath();

        if (string.IsNullOrEmpty(_ffmpegPath))
        {
            _logger.LogInformation("System FFmpeg not found. Downloading FFmpeg binaries (one-time, ~100MB)...");

            try
            {
                var downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg");
                Directory.CreateDirectory(downloadsPath);

                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, downloadsPath);
                FFmpeg.SetExecutablesPath(downloadsPath);

                _ffmpegPath = FFmpeg.ExecutablesPath;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _ffmpegPath = Path.Combine(_ffmpegPath, "ffmpeg.exe");
                else
                    _ffmpegPath = Path.Combine(_ffmpegPath, "ffmpeg");

                _logger.LogInformation("FFmpeg downloaded successfully to: {path}", _ffmpegPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download FFmpeg. Camera streaming will not work.");
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }
        }
        else
        {
            _logger.LogInformation("Found system FFmpeg at: {path}", _ffmpegPath);
        }

        var tasks = new List<Task>();
        foreach (var camera in _settings.Cameras.Where(c => c.Enabled))
        {
            var cameraStream = new CameraStream(camera);
            _cameraStreams[camera.Id] = cameraStream;
            
            // Start camera stream task
            tasks.Add(RunCameraStreamAsync(camera, cameraStream, stoppingToken));
            
            // Start health monitor task
            tasks.Add(MonitorCameraHealthAsync(camera, cameraStream, stoppingToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task MonitorCameraHealthAsync(Camera camera, CameraStream stream, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting health monitor for camera {id}", camera.Id);
        
        // Give the camera time to connect initially
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(HEALTH_CHECK_INTERVAL_MS, stoppingToken);

                var timeSinceLastFrame = DateTime.UtcNow - stream.LastFrameTime;
                var isStalled = stream.IsConnected && 
                               timeSinceLastFrame.TotalSeconds > FRAME_TIMEOUT_SECONDS;

                // Check if FFmpeg process died unexpectedly
                var processAlive = stream.FfmpegProcess != null && !stream.FfmpegProcess.HasExited;

                if (isStalled)
                {
                    _logger.LogWarning(
                        "Camera {id} health check FAILED: No frames received for {seconds} seconds. Forcing reconnect...",
                        camera.Id, 
                        (int)timeSinceLastFrame.TotalSeconds
                    );
                    
                    // Force reconnection by canceling the stream
                    stream.StreamCts?.Cancel();
                }
                else if (stream.IsConnected && !processAlive)
                {
                    _logger.LogWarning(
                        "Camera {id} health check FAILED: FFmpeg process died unexpectedly. Forcing reconnect...",
                        camera.Id
                    );
                    
                    stream.IsConnected = false;
                    stream.StreamCts?.Cancel();
                }
                else if (stream.IsConnected && stream.LatestFrame.Length > 0)
                {
                    // Only log healthy status every 30 seconds to avoid spam
                    if (timeSinceLastFrame.TotalSeconds < 3)
                    {
                        _logger.LogTrace(
                            "Camera {id} health check OK: Frame received {seconds}s ago, {size} bytes",
                            camera.Id,
                            (int)timeSinceLastFrame.TotalSeconds,
                            stream.LatestFrame.Length
                        );
                    }
                }
                else if (stream.IsConnected)
                {
                    _logger.LogDebug(
                        "Camera {id}: Connected but waiting for first frame ({seconds}s elapsed)...",
                        camera.Id,
                        (int)timeSinceLastFrame.TotalSeconds
                    );
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in health monitor for camera {id}", camera.Id);
            }
        }

        _logger.LogInformation("Health monitor stopped for camera {id}", camera.Id);
    }

    private async Task RunCameraStreamAsync(Camera camera, CameraStream stream, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting camera stream for {name} ({id}) at {url}", 
            camera.Name, camera.Id, camera.GetRtspUrl());

        var reconnectAttempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                reconnectAttempt++;
                
                if (reconnectAttempt > 1)
                {
                    _logger.LogInformation(
                        "Camera {id}: Reconnection attempt #{attempt}",
                        camera.Id, 
                        reconnectAttempt
                    );
                }

                await ConnectAndStreamAsync(camera, stream, stoppingToken);
                
                // If we got here, the stream ended normally or was cancelled
                reconnectAttempt = 0; // Reset counter on clean disconnect
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Camera {id} stream stopping", camera.Id);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Camera {id} stream error (attempt #{attempt}), reconnecting in {delay}ms", 
                    camera.Id, 
                    reconnectAttempt,
                    _settings.ReconnectDelayMs);
                    
                stream.IsConnected = false;
                
                // Exponential backoff for repeated failures (max 30 seconds)
                var delay = Math.Min(_settings.ReconnectDelayMs * (int)Math.Pow(2, Math.Min(reconnectAttempt - 1, 4)), 30000);
                _logger.LogDebug("Camera {id}: Using reconnect delay of {delay}ms", camera.Id, delay);
                
                await Task.Delay(delay, stoppingToken);
            }
            finally
            {
                CleanupStream(stream);
            }
        }
    }

    private string? FindFFmpegPath()
    {
        var possibleNames = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "ffmpeg.exe", "ffmpeg" }
            : new[] { "ffmpeg" };

        foreach (var name in possibleNames)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                    Arguments = name,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        var path = output.Split('\n', '\r')[0].Trim();
                        if (File.Exists(path))
                            return path;
                    }
                }
            }
            catch
            {
                // Continue to next option
            }
        }

        return null;
    }

    private async Task ConnectAndStreamAsync(Camera camera, CameraStream stream, CancellationToken token)
    {
        var rtspUrl = camera.GetRtspUrl();

        if (!string.IsNullOrWhiteSpace(camera.Username) && !string.IsNullOrWhiteSpace(camera.Password))
        {
            var uri = new Uri(rtspUrl);
            rtspUrl = $"{uri.Scheme}://{camera.Username}:{camera.Password}@{uri.Host}:{uri.Port}{uri.PathAndQuery}";
        }

        _logger.LogInformation("Camera {id}: Connecting to RTSP stream: {url}", 
            camera.Id, camera.GetRtspUrl());

        stream.StreamCts = new CancellationTokenSource();
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, stream.StreamCts.Token);

        // Improved FFmpeg arguments for better reliability
        var ffmpegArgs = $"-rtsp_transport tcp " +
                        $"-stimeout 5000000 " +           // 5 second socket timeout
                        $"-max_delay 500000 " +           // Max demux delay
                        $"-fflags +genpts+discardcorrupt " + // Generate PTS, discard corrupt packets
                        $"-i \"{rtspUrl}\" " +
                        $"-f mjpeg " +
                        $"-q:v 5 " +                      // Quality
                        $"-r 15 " +                       // Frame rate
                        $"-nostdin " +                    // Don't read stdin
                        $"-";

        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath!,
            Arguments = ffmpegArgs,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        stream.FfmpegProcess = new Process { StartInfo = startInfo };

        stream.FfmpegProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Camera {id} FFmpeg: {message}", camera.Id, e.Data);
                }
                else if (e.Data.Contains("Stream", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("Duration", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("Input", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Camera {id} FFmpeg: {message}", camera.Id, e.Data);
                }
                else
                {
                    _logger.LogDebug("Camera {id} FFmpeg: {message}", camera.Id, e.Data);
                }
            }
        };

        stream.FfmpegProcess.Start();
        
        // Close stdin to prevent FFmpeg from waiting for input
        stream.FfmpegProcess.StandardInput.Close();
        
        stream.FfmpegProcess.BeginErrorReadLine();
        stream.IsConnected = true;
        
        // Reset frame tracking for new connection
        stream.LastFrameTime = DateTime.UtcNow;

        _logger.LogInformation("Camera {id}: FFmpeg process started (PID: {pid}), reading MJPEG stream", 
            camera.Id, stream.FfmpegProcess.Id);

        try
        {
            await ReadMjpegStreamAsync(camera, stream, stream.FfmpegProcess.StandardOutput.BaseStream, combinedCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Camera {id}: Error reading MJPEG stream", camera.Id);
            throw;
        }
        finally
        {
            _logger.LogInformation("Camera {id}: Stream reading ended", camera.Id);
        }
    }

    private async Task ReadMjpegStreamAsync(Camera camera, CameraStream cameraStream, Stream stream, CancellationToken token)
    {
        var buffer = new byte[1024 * 1024];
        var frameBuffer = new MemoryStream();
        var readingFrame = false;

        byte[] jpegStart = { 0xFF, 0xD8 };
        byte[] jpegEnd = { 0xFF, 0xD9 };

        var frameCount = 0;
        var lastLogTime = DateTime.UtcNow;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var bytesRead = await stream.ReadAsync(buffer, token);
                
                if (bytesRead == 0)
                {
                    _logger.LogWarning("Camera {id}: Stream ended (0 bytes read)", camera.Id);
                    break;
                }

                for (int i = 0; i < bytesRead; i++)
                {
                    if (!readingFrame && i < bytesRead - 1 &&
                        buffer[i] == jpegStart[0] && buffer[i + 1] == jpegStart[1])
                    {
                        readingFrame = true;
                        frameBuffer = new MemoryStream();
                        frameBuffer.WriteByte(buffer[i]);
                    }
                    else if (readingFrame)
                    {
                        frameBuffer.WriteByte(buffer[i]);

                        if (i > 0 && buffer[i - 1] == jpegEnd[0] && buffer[i] == jpegEnd[1])
                        {
                            var frame = frameBuffer.ToArray();
                            frameCount++;

                            lock (cameraStream.FrameLock)
                            {
                                cameraStream.LatestFrame = frame;
                                cameraStream.LastFrameTime = DateTime.UtcNow;
                            }

                            cameraStream.FrameSemaphore.Release();

                            // Log frame rate every 10 seconds
                            var timeSinceLog = DateTime.UtcNow - lastLogTime;
                            if (timeSinceLog.TotalSeconds >= 10)
                            {
                                var fps = frameCount / timeSinceLog.TotalSeconds;
                                _logger.LogInformation(
                                    "Camera {id}: Receiving frames at {fps:F1} FPS (frame size: {size} bytes)",
                                    camera.Id, fps, frame.Length
                                );
                                frameCount = 0;
                                lastLogTime = DateTime.UtcNow;
                            }

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await _cameraDataStreamService.PublishFrameAsync(camera.Id, frame, CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Camera {id}: Error publishing frame to data stream", camera.Id);
                                }
                            });

                            _logger.LogTrace("Camera {id}: Frame captured: {size} bytes", camera.Id, frame.Length);

                            readingFrame = false;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Camera {id}: Stream reading cancelled", camera.Id);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Camera {id}: Error reading stream", camera.Id);
                throw;
            }
        }

        _logger.LogInformation("Camera {id}: Exiting stream reader (total frames: {count})", camera.Id, frameCount);
    }

    public async IAsyncEnumerable<byte[]> StreamFramesAsync(
        string cameraId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_cameraStreams.TryGetValue(cameraId, out var stream))
        {
            _logger.LogWarning("Client requested stream for non-existent camera: {id}", cameraId);
            yield break;
        }

        _logger.LogInformation("Client subscribed to camera {id} stream", cameraId);

        while (!cancellationToken.IsCancellationRequested)
        {
            bool success = await stream.FrameSemaphore.WaitAsync(5000, cancellationToken);

            if (!success)
                continue;

            byte[] frame;
            lock (stream.FrameLock)
            {
                frame = (byte[])stream.LatestFrame.Clone();
            }

            if (frame.Length > 0)
            {
                yield return frame;
            }
        }

        _logger.LogInformation("Client unsubscribed from camera {id} stream", cameraId);
    }

    private void CleanupStream(CameraStream stream)
    {
        try
        {
            stream.StreamCts?.Cancel();
            stream.StreamCts?.Dispose();
            stream.StreamCts = null;

            if (stream.FfmpegProcess != null)
            {
                try
                {
                    if (!stream.FfmpegProcess.HasExited)
                    {
                        _logger.LogDebug("Killing FFmpeg process {pid} for camera {id}", 
                            stream.FfmpegProcess.Id, stream.Camera.Id);
                        stream.FfmpegProcess.Kill(true);
                        stream.FfmpegProcess.WaitForExit(2000);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Process already exited
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error killing FFmpeg process for camera {id}", stream.Camera.Id);
                }

                try
                {
                    stream.FfmpegProcess.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }

                stream.FfmpegProcess = null;
            }

            stream.IsConnected = false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup for camera {id}", stream.Camera.Id);
        }
    }

    public override void Dispose()
    {
        foreach (var stream in _cameraStreams.Values)
        {
            CleanupStream(stream);
            stream.FrameSemaphore.Dispose();
        }
        _cameraStreams.Clear();
        base.Dispose();
    }
}

public class CameraStream
{
    public Camera Camera { get; set; }
    public object FrameLock { get; } = new();
    public byte[] LatestFrame { get; set; } = Array.Empty<byte>();
    public SemaphoreSlim FrameSemaphore { get; } = new(0);
    public Process? FfmpegProcess { get; set; }
    public CancellationTokenSource? StreamCts { get; set; }
    public bool IsConnected { get; set; }
    public DateTime LastFrameTime { get; set; } = DateTime.MinValue;

    public CameraStream(Camera camera)
    {
        Camera = camera;
    }
}

public class CameraStreamStatus
{
    public string CameraId { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public bool HasFrame { get; set; }
    public int FrameSize { get; set; }
    public DateTime LastFrameTime { get; set; }
    public bool IsHealthy { get; set; }
    public int SecondsSinceLastFrame { get; set; }
}