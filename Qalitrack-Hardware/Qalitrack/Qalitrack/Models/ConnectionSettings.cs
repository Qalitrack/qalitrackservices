using System.IO.Ports;
using System.Text.Json.Serialization;

namespace Qalitrack.Models;

public class ConnectionSettings
{
    public string ConnectionType { get; set; } = "TCP"; // "Serial" or "TCP"
    
    // Serial settings
    public string SerialPort { get; set; } = "/dev/ttyUSB0";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; } = StopBits.One;
    
    // TCP settings
    public string IpAddress { get; set; } = "172.16.1.243";
    public int Port { get; set; } = 3002;
    public int ReadTimeoutMs { get; set; } = 1000;
    public int ReconnectDelayMs { get; set; } = 5000;

    // TcpListener settings (for configuration binding)
    [JsonIgnore]
    public TcpListenerSettings TcpListener { get; set; } = new();

    // Bind TcpListener properties to root level
    public void BindTcpListener()
    {
        if (TcpListener != null)
        {
            if (!string.IsNullOrEmpty(TcpListener.IpAddress))
                IpAddress = TcpListener.IpAddress;
                
            if (TcpListener.Port != default)
                Port = TcpListener.Port;
                
            if (TcpListener.ReadTimeoutMs != default)
                ReadTimeoutMs = TcpListener.ReadTimeoutMs;
                
            if (TcpListener.ReconnectDelayMs != default)
                ReconnectDelayMs = TcpListener.ReconnectDelayMs;
        }
    }
}
