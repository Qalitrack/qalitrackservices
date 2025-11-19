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
            return new CameraStreamStatus
            {
                CameraId = cameraId,
                IsConnected = stream.IsConnected,
                HasFrame = stream.LatestFrame.Length > 0,
                FrameSize = stream.LatestFrame.Length,
                LastFrameTime = stream.LastFrameTime
            };
        }
        return new CameraStreamStatus { CameraId = cameraId, IsConnected = false };
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
            tasks.Add(RunCameraStreamAsync(camera, cameraStream, stoppingToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task RunCameraStreamAsync(Camera camera, CameraStream stream, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting camera stream for {name} ({id}) at {url}", camera.Name, camera.Id, camera.GetRtspUrl());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndStreamAsync(camera, stream, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Camera {id} stream stopping", camera.Id);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Camera {id} stream error, reconnecting in {delay}ms", camera.Id, _settings.ReconnectDelayMs);
                stream.IsConnected = false;
                await Task.Delay(_settings.ReconnectDelayMs, stoppingToken);
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

        _logger.LogInformation("Camera {id}: Connecting to RTSP stream: {url}", camera.Id, camera.GetRtspUrl());

        stream.StreamCts = new CancellationTokenSource();
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, stream.StreamCts.Token);

        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath!,
            Arguments = $"-rtsp_transport tcp -i \"{rtspUrl}\" -f mjpeg -q:v 5 -r 15 -",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        stream.FfmpegProcess = new Process { StartInfo = startInfo };

        stream.FfmpegProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Camera {id} FFmpeg: {message}", camera.Id, e.Data);
                }
                else if (e.Data.Contains("Stream", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("Duration", StringComparison.OrdinalIgnoreCase))
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
        stream.FfmpegProcess.BeginErrorReadLine();
        stream.IsConnected = true;

        _logger.LogInformation("Camera {id}: FFmpeg process started (PID: {pid}), reading MJPEG stream", camera.Id, stream.FfmpegProcess.Id);

        await ReadMjpegStreamAsync(camera, stream, stream.FfmpegProcess.StandardOutput.BaseStream, combinedCts.Token);
    }

    private async Task ReadMjpegStreamAsync(Camera camera, CameraStream cameraStream, Stream stream, CancellationToken token)
    {
        var buffer = new byte[1024 * 1024];
        var frameBuffer = new MemoryStream();
        var readingFrame = false;

        byte[] jpegStart = { 0xFF, 0xD8 };
        byte[] jpegEnd = { 0xFF, 0xD9 };

        while (!token.IsCancellationRequested)
        {
            try
            {
                var bytesRead = await stream.ReadAsync(buffer, token);
                if (bytesRead == 0) break;

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

                            lock (cameraStream.FrameLock)
                            {
                                cameraStream.LatestFrame = frame;
                                cameraStream.LastFrameTime = DateTime.UtcNow;
                            }

                            cameraStream.FrameSemaphore.Release();

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
                break;
            }
        }
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
                        stream.FfmpegProcess.Kill(true);
                        stream.FfmpegProcess.WaitForExit(2000);
                    }
                }
                catch (InvalidOperationException)
                {
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
                }

                stream.FfmpegProcess = null;
            }
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
    public DateTime LastFrameTime { get; set; }

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
}
