using System.Text.RegularExpressions;

namespace Qalitrack.Services.ScaleProtocols;

// Parses Yaghua XK3190-DS8 serial output:
//   "st, gs, + 000000kg"
//    ^    ^   ^  ^     ^
//    |    |   |  value unit
//    |    |   sign (+ / -)
//    |    mode: gs=gross, nt=net
//    status: st=stable, us=unstable
public class YaghuaXK3190DS8Protocol : IScaleProtocol
{
    public bool PublishRawWeight => true;
    private static readonly Regex _pattern = new(
        @"^(st|us),\s*(gs|nt),\s*([+\-])\s*(\d+(?:\.\d+)?)\s*(kg|t|lb|g)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ScaleReading Parse(string line)
    {
        var m = _pattern.Match(line.Trim());
        if (!m.Success)
            return new ScaleReading("unknown", "", line, line);

        var stable = m.Groups[1].Value.Equals("st", StringComparison.OrdinalIgnoreCase);
        var gross  = m.Groups[2].Value.Equals("gs", StringComparison.OrdinalIgnoreCase);
        var sign   = m.Groups[3].Value;
        var number = m.Groups[4].Value;
        var unit   = m.Groups[5].Value;

        var display = $"{sign}{number}{unit} ({(stable ? "stable" : "unstable")}, {(gross ? "gross" : "net")})";

        // Weight = digits only — published as a plain string to the stream
        return new ScaleReading("weight", "", number, display);
    }
}
