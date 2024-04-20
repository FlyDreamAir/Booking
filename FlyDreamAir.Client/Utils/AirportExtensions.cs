using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Client.Utils;

public static class AirportExtensions
{
    public static TimeZoneInfo GetTimeZone(this Airport airport)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(airport.TimeZone);
    }

    public static DateTimeOffset GetLocalTime(this Airport airport, DateTimeOffset? time)
    {
        return TimeZoneInfo.ConvertTime(time ?? DateTimeOffset.Now, airport.GetTimeZone());
    }

    public static DateTimeOffset GetLocalDate(this Airport airport, DateTimeOffset? date)
    {
        return new DateTimeOffset(
            (date ?? DateTimeOffset.Now).Date,
            airport.GetTimeZone().BaseUtcOffset);
    }
}
