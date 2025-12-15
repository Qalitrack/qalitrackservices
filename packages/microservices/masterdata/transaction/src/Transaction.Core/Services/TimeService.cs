using Transaction.Core.Interfaces;

namespace Transaction.Core.Services;

/// <summary>
/// Service for handling timezone-aware date and time operations.
/// Configured for East African Time (Africa/Nairobi, UTC+3)
/// </summary>
public class TimeService : ITimeService
{
    private readonly TimeZoneInfo _timeZone;

    public TimeService()
    {
        // East African Time (Nairobi, Kenya)
        // UTC+3, no daylight saving time
        try
        {
            // Try IANA timezone ID (works on Linux/Mac)
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                // Fallback to Windows timezone ID
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Final fallback: Create custom timezone for EAT (UTC+3)
                _timeZone = TimeZoneInfo.CreateCustomTimeZone(
                    id: "East Africa Time",
                    baseUtcOffset: TimeSpan.FromHours(3),
                    displayName: "East Africa Time",
                    standardDisplayName: "EAT"
                );
            }
        }
    }

    /// <summary>
    /// Constructor with custom timezone (for testing)
    /// </summary>
    public TimeService(TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;
    }

    /// <summary>
    /// Gets the current date and time in East African Time as UTC
    /// </summary>
    public DateTime Now => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone), DateTimeKind.Utc);

    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Gets the configured timezone (East African Time)
    /// </summary>
    public TimeZoneInfo TimeZone => _timeZone;

    /// <summary>
    /// Converts a UTC DateTime to East African Time
    /// </summary>
    public DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            // Assume it's UTC if unspecified
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
    }

    /// <summary>
    /// Converts an East African Time DateTime to UTC
    /// </summary>
    public DateTime ConvertToUtc(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
        {
            return localDateTime;
        }

        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, _timeZone);
    }
}
