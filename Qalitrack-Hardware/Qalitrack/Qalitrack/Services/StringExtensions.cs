namespace Qalitrack.Services;

public static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength)
        => value?.Length > maxLength ? value.Substring(0, maxLength) + "..." : value ?? "";
}