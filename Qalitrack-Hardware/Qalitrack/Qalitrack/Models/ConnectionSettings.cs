using System.IO.Ports;
using System.Text.Json.Serialization;

namespace Qalitrack.Models;

public class ConnectionSettings
{
    public string ConnectionType { get; set; } = "TCP"; // "Serial" or "TCP"
    
    // Serial settings
    public string SerialPort { get; set; } = "AUTO"; // "AUTO", "/dev/ttyUSB0", or "COM9"
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
                
            if (TcpListener.Port > 0)
                Port = TcpListener.Port;
                
            if (TcpListener.ReadTimeoutMs > 0)
                ReadTimeoutMs = TcpListener.ReadTimeoutMs;
                
            if (TcpListener.ReconnectDelayMs > 0)
                ReconnectDelayMs = TcpListener.ReconnectDelayMs;
        }
    }

    // Validation method
    public bool IsValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ConnectionType))
        {
            errorMessage = "ConnectionType cannot be empty";
            return false;
        }

        var type = ConnectionType.ToUpperInvariant();
        
        if (type == "SERIAL" || type == "AUTO")
        {
            if (string.IsNullOrWhiteSpace(SerialPort))
            {
                errorMessage = "SerialPort cannot be empty for Serial connections";
                return false;
            }

            if (BaudRate <= 0)
            {
                errorMessage = "BaudRate must be positive";
                return false;
            }

            if (DataBits < 5 || DataBits > 8)
            {
                errorMessage = "DataBits must be between 5 and 8";
                return false;
            }
        }
        
        if (type == "TCP" || type == "AUTO")
        {
            if (string.IsNullOrWhiteSpace(IpAddress) && type == "TCP")
            {
                errorMessage = "IpAddress cannot be empty for TCP connections";
                return false;
            }

            if (Port <= 0 || Port > 65535)
            {
                errorMessage = "Port must be between 1 and 65535";
                return false;
            }
        }

        if (ReadTimeoutMs < 0)
        {
            errorMessage = "ReadTimeoutMs cannot be negative";
            return false;
        }

        if (ReconnectDelayMs < 0)
        {
            errorMessage = "ReconnectDelayMs cannot be negative";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    // Get display-friendly connection info
    public string GetConnectionDescription()
    {
        var type = ConnectionType.ToUpperInvariant();
        
        return type switch
        {
            "SERIAL" => $"Serial: {SerialPort} @ {BaudRate} baud",
            "TCP" => $"TCP: {IpAddress}:{Port}",
            "AUTO" => $"Auto (TCP: {IpAddress}:{Port}, Serial: {SerialPort} @ {BaudRate} baud)",
            _ => $"Unknown: {ConnectionType}"
        };
    }
    
}