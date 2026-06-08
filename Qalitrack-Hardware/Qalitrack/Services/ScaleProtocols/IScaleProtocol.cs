namespace Qalitrack.Services.ScaleProtocols;

public record ScaleReading(
    string Type,           // "platform", "total", "weight", "unknown"
    string PlatformNumber, // empty when not applicable
    string Weight,
    string DisplayLine
);

public interface IScaleProtocol
{
    ScaleReading Parse(string line);

    // When true, ProcessPlatformLine publishes Weight as a plain string instead of a JSON object.
    bool PublishRawWeight { get; }
}
