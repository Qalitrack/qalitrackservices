namespace Qalitrack.Models;

public class Camera
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 554;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "admin";
    public string RtspPath { get; set; } = "/";
    public bool Enabled { get; set; } = true;
    public bool SupportsSnapshot { get; set; } = true;
    public NprSettings? NprSettings { get; set; }

    public string GetRtspUrl()
    {
        return $"rtsp://{IpAddress}:{Port}{RtspPath}";
    }
}

public class NprSettings
{
    public bool Enabled { get; set; } = false;
    public string CameraQueryUrl { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int PollingIntervalMs { get; set; } = 1000;
    public bool UseWebSocket { get; set; } = true;
}

public class CameraSettings
{
    public List<Camera> Cameras { get; set; } = new List<Camera>();
    public int ReconnectDelayMs { get; set; } = 5000;
    public int FrameBufferSize { get; set; } = 100;
}
