using System.Text.RegularExpressions;

namespace Qalitrack.Services.ScaleProtocols;

// Parses the original multi-platform scale format:
//   "Platform 1: 500.00kg"
//   "Total: 1000.00kg"
public class GenericScaleProtocol : IScaleProtocol
{
    public bool PublishRawWeight => false;
    private static readonly Regex _platformPattern = new(
        @"Platform\s+(\d+)\s*:\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _totalPattern = new(
        @"Total\s*:\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ScaleReading Parse(string line)
    {
        var platformMatch = _platformPattern.Match(line);
        if (platformMatch.Success)
        {
            var number  = platformMatch.Groups[1].Value;
            var weight  = platformMatch.Groups[2].Value.Trim();
            return new ScaleReading("platform", number, weight, $"Platform {number}: {weight}");
        }

        var totalMatch = _totalPattern.Match(line);
        if (totalMatch.Success)
        {
            var weight = totalMatch.Groups[1].Value.Trim();
            return new ScaleReading("total", "", weight, $"Total: {weight}");
        }

        return new ScaleReading("unknown", "", line, line);
    }
}
