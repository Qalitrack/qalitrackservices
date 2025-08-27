using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace QaliTrack.MasterData.Api.Infrastructure;

public class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;
        
        // Convert PascalCase to lowercase with hyphens (Django-style)
        return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}