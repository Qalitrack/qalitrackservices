namespace Qalitrack.Models;

public class RfidSettings
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "172.16.0.243";
    public int Port { get; set; } = 2022;
    public int ScanIntervalMs { get; set; } = 1500;
    public int ReconnectDelayMs { get; set; } = 5000;
}