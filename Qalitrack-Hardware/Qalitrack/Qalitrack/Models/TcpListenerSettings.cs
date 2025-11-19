namespace Qalitrack.Models;

public class TcpListenerSettings
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 3002;
    public int ReadTimeoutMs { get; set; } = 1000;
    public int ReconnectDelayMs { get; set; } = 5000;
}
