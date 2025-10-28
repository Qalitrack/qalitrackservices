namespace Transaction.Core.Interfaces;

/// <summary>
/// Service for handling timezone-aware date and time operations
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Gets the current date and time in the configured timezone (East African Time)
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Converts a UTC DateTime to the configured timezone
    /// </summary>
    DateTime ConvertFromUtc(DateTime utcDateTime);

    /// <summary>
    /// Converts a local DateTime to UTC
    /// </summary>
    DateTime ConvertToUtc(DateTime localDateTime);

    /// <summary>
    /// Gets the configured timezone info
    /// </summary>
    TimeZoneInfo TimeZone { get; }
}
