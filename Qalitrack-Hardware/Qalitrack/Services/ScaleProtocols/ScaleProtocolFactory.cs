using Qalitrack.Models;

namespace Qalitrack.Services.ScaleProtocols;

public static class ScaleProtocolFactory
{
    public static IScaleProtocol Create(ScaleType scaleType) => scaleType switch
    {
        ScaleType.YaghuaXK3190DS8 => new YaghuaXK3190DS8Protocol(),
        _                          => new GenericScaleProtocol()
    };
}
