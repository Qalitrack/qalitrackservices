using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace Qalitrack.Services;

public class CameraDataStreamService
{
    private readonly ConcurrentDictionary<string, Channel<string>> _streams = new();
    private readonly ILogger<CameraDataStreamService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CameraDataStreamService(ILogger<CameraDataStreamService> logger)
    {
        _logger = logger;
    }

    public async Task PublishFrameAsync(string cameraId, byte[] frameData, CancellationToken cancellationToken = default)
    {
        var base64Frame = Convert.ToBase64String(frameData);
        var payload = new
        {
            cameraId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            frameSize = frameData.Length,
            frame = base64Frame
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        foreach (var (clientId, channel) in _streams)
        {
            try
            {
                await channel.Writer.WriteAsync(json, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing camera {CameraId} frame to client {ClientId}", cameraId, clientId);
                _streams.TryRemove(clientId, out _);
            }
        }
    }

    public IAsyncEnumerable<string> SubscribeAsync(string clientId, string? cameraId = null, CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var streamKey = string.IsNullOrEmpty(cameraId) ? clientId : $"{clientId}_{cameraId}";
        _streams[streamKey] = channel;
        _logger.LogInformation("Client {ClientId} subscribed to camera data stream (camera: {CameraId})", clientId, cameraId ?? "all");

        cancellationToken.Register(() =>
        {
            Unsubscribe(streamKey);
        });

        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    public void Unsubscribe(string clientId)
    {
        if (_streams.TryRemove(clientId, out var channel))
        {
            _logger.LogInformation("Client {ClientId} unsubscribed from camera data stream", clientId);
            channel.Writer.TryComplete();
        }
    }

    public int GetClientCount() => _streams.Count;
}
