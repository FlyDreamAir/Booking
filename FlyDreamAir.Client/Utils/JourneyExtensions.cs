using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Client.Utils;

public static class JourneyExtensions
{
    public static DateTimeOffset GetDepartureTime(this Journey journey)
    {
        return journey.Flights.First().From.GetLocalTime(
            journey.Flights.First().DepartureTime);
    }

    public static DateTimeOffset GetArrivalTime(this Journey journey)
    {
        return journey.Flights.Last().To.GetLocalTime(
            journey.Flights.Last().DepartureTime +
            journey.Flights.Last().EstimatedTime
        );
    }
}
