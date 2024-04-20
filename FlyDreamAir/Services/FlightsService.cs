using Model = FlyDreamAir.Data.Model;
using FlyDreamAir.Data;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Services;

public class FlightsService
{
    private const int MAX_CONNECTING_FLIGHTS = 4;

    private readonly AirportsService _airportsService;
    private readonly ApplicationDbContext _dbContext;

    public FlightsService(
        AirportsService airportsService,
        DbContextOptions<ApplicationDbContext> dbContextOptions
    )
    {
        _airportsService = airportsService;
        _dbContext = new(dbContextOptions);
    }

    public async Task<Model.Flight?> GetFlightAsync(
        string flightId,
        DateTimeOffset departureTime,
        bool searchPast
    )
    {
        departureTime = departureTime.ToUniversalTime();

        if (searchPast && departureTime < DateTimeOffset.UtcNow)
        {
            return null;
        }

        var scheduledFlight = await _dbContext.ScheduledFlights.Include(sf => sf.Flight)
            .FirstOrDefaultAsync(sf =>
                sf.Flight.FlightId == flightId && sf.DepartureTime == departureTime);

        if (scheduledFlight is not null)
        {
            return new Model.Flight()
            {
                FlightId = scheduledFlight.Flight.FlightId,
                From = await _airportsService.GetAirportAsync(scheduledFlight.Flight.FromAirport)
                    ?? throw new InvalidOperationException("Invalid from airport"),
                To = await _airportsService.GetAirportAsync(scheduledFlight.Flight.ToAirport)
                    ?? throw new InvalidOperationException("Invalid to airport"),
                BaseCost = scheduledFlight.Flight.BaseCost,
                DepartureTime = scheduledFlight.DepartureTime,
                EstimatedTime = scheduledFlight.Flight.EstimatedTime
            };
        }
        else
        {
            return null;
        }
    }

    public async IAsyncEnumerable<Model.Journey> GetJourneysAsync(
        string fromCode,
        string toCode,
        DateTimeOffset date,
        DateTimeOffset? returnDate
    )
    {
        if (date < DateTimeOffset.UtcNow)
        {
            yield break;
        }

        if (returnDate.HasValue && returnDate <= date)
        {
            yield break;
        }

        date = date.ToUniversalTime();
        returnDate = returnDate.HasValue ? returnDate.Value.ToUniversalTime() : returnDate;

        var isTwoWay = returnDate.HasValue;

        var fromAirport = await _airportsService.GetAirportAsync(fromCode);
        var toAirport = await _airportsService.GetAirportAsync(toCode);

        if (fromAirport is null || toAirport is null)
        {
            yield break;
        }

        await foreach (var journey in FindJourney(
            fromAirport, date,
            new(), new(), new(), false
        ))
        {
            yield return journey;
        }

        async IAsyncEnumerable<Model.Journey> FindJourney(
            Model.Airport currentFromAirport,
            DateTimeOffset currentDate,
            List<Model.Flight> flights,
            List<Model.Flight> returnFlights,
            HashSet<string> excludedAirports,
            bool isReturn
        )
        {
            Model.Journey ConstructJourney()
            {
                TimeSpan GetEstimatedTime(List<Model.Flight> flights)
                {
                    if (!flights.Any())
                    {
                        return default;
                    }
                    return flights.Last().DepartureTime + flights.Last().EstimatedTime
                        - flights.First().DepartureTime;
                }

                return new()
                {
                    From = fromAirport!,
                    To = toAirport!,
                    BaseCost = flights.Concat(returnFlights).Sum(f => f.BaseCost),
                    IsTwoWay = isTwoWay,
                    EstimatedTime = GetEstimatedTime(flights),
                    ReturnEstimatedTime = GetEstimatedTime(returnFlights),
                    // Be sure to clone these lists!
                    Flights = new(flights),
                    ReturnFlights = new(returnFlights)
                };
            }

            var currentDestination = isReturn ? fromAirport! : toAirport!;
            var currentFlightsList = isReturn ? returnFlights : flights;
            var level = isReturn ? returnFlights.Count : flights.Count;
            if (level >= MAX_CONNECTING_FLIGHTS)
            {
                yield break;
            }

            excludedAirports.Add(currentFromAirport.Id);

            var scheduledFlights = _dbContext.ScheduledFlights.Include(sf => sf.Flight)
                .Where(sf => sf.DepartureTime >= currentDate
                        && sf.DepartureTime <= currentDate.AddDays(1)
                        && sf.Flight.FromAirport == currentFromAirport.Id
                        && !excludedAirports.Contains(sf.Flight.ToAirport));

            foreach (var scheduledFlight in await scheduledFlights.ToListAsync())
            {
                var currentToAirport = await _airportsService
                    .GetAirportAsync(scheduledFlight.Flight.ToAirport);

                if (currentToAirport is null)
                {
                    continue;
                }

                var currentArrivalDate = scheduledFlight.DepartureTime
                    + scheduledFlight.Flight.EstimatedTime;

                currentFlightsList.Add(new()
                {
                    FlightId = scheduledFlight.Flight.FlightId,
                    From = currentFromAirport,
                    To = currentToAirport,
                    EstimatedTime = scheduledFlight.Flight.EstimatedTime,
                    DepartureTime = scheduledFlight.DepartureTime,
                    BaseCost = scheduledFlight.Flight.BaseCost,
                });

                IAsyncEnumerable<Model.Journey>? enumerable = null;

                if (scheduledFlight.Flight.ToAirport == currentDestination.Id)
                {
                    if (isTwoWay && !isReturn)
                    {
                        enumerable = FindJourney(
                            currentToAirport, currentArrivalDate,
                            flights, returnFlights, new(), true);
                    }
                    else
                    {
                        yield return ConstructJourney();
                    }
                }
                else
                {
                    enumerable = FindJourney(
                        currentToAirport, currentArrivalDate,
                        flights, returnFlights, excludedAirports, isReturn);
                }

                if (enumerable is not null)
                {
                    await foreach (var journey in enumerable)
                    {
                        yield return journey;
                    }
                }

                currentFlightsList.RemoveAt(currentFlightsList.Count - 1);
            }

            excludedAirports.Remove(currentFromAirport.Id);
        }
    }
}
