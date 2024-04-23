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

    public static Journey ToJourney(this IEnumerable<Flight> allFlights,
        string from,
        string to,
        DateTime date,
        DateTime? returnDate
    )
    {
        List<Flight> flights;
        List<Flight> returnFlights = [];

        static TimeSpan GetEstimatedTime(List<Flight> list)
        {
            if (list.Count == 0)
            {
                return default;
            }
            return list.Last().DepartureTime + list.Last().EstimatedTime
                - list.First().DepartureTime;
        }

        if (!returnDate.HasValue)
        {
            flights = allFlights.ToList();
        }
        else
        {
            flights = allFlights.Where(f => f.DepartureTime < returnDate).ToList();
            returnFlights = allFlights.Where(f => f.DepartureTime >= returnDate).ToList();
        }

        return new Journey()
        {
            From = flights.First().From,
            To = flights.Last().To,
            IsTwoWay = returnDate.HasValue,
            BaseCost = flights.Sum(f => f.BaseCost) + returnFlights.Sum(f => f.BaseCost),
            EstimatedTime = GetEstimatedTime(flights),
            ReturnEstimatedTime = GetEstimatedTime(returnFlights),
            Flights = flights,
            ReturnFlights = returnFlights
        };
    }
}
