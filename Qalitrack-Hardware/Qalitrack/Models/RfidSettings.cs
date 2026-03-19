namespace Qalitrack.Models;

public class RfidSettings
{
    public bool   Enabled          { get; set; } = false;
    public string Host             { get; set; } = "";
    public int    Port             { get; set; } = 0;
    public int    ScanIntervalMs   { get; set; } = 1500;
    public int    ReconnectDelayMs { get; set; } = 5000;
}