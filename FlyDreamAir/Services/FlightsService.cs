using Model = FlyDreamAir.Data.Model;
using FlyDreamAir.Data;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Services;

public class FlightsService
{
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

    public async IAsyncEnumerable<Model.Journey> GetJourneysAsync(
        string fromCode,
        string toCode,
        DateTime date
    )
    {
        if (date < DateTime.Now)
        {
            yield break;
        }

        var fromAirport = await _airportsService.GetAirportAsync(fromCode);
        var toAirport = await _airportsService.GetAirportAsync(toCode);

        if (fromAirport is null || toAirport is null)
        {
            yield break;
        }

        var scheduledFlights = _dbContext.ScheduledFlights.Include(sf => sf.Flight)
            .Where(sf => sf.DepartureTime >= date
                    && sf.DepartureTime <= date.AddDays(1)
                    && sf.Flight.FromAirport == fromCode
                    && sf.Flight.ToAirport == toCode);

        await foreach (var scheduledFlight in scheduledFlights.ToAsyncEnumerable())
        {
            yield return new()
            {
                From = fromAirport,
                To = toAirport,
                BaseCost = scheduledFlight.Flight.BaseCost,
                IsTwoWay = false,
                EstimatedTime = scheduledFlight.Flight.EstimatedTime,
                Flights = [
                    new()
                    {
                        FlightId = scheduledFlight.Flight.FlightId,
                        From = fromAirport,
                        To = toAirport,
                        EstimatedTime = scheduledFlight.Flight.EstimatedTime,
                        DepartureTime = scheduledFlight.DepartureTime
                    }
                ],
                ReturnEstimatedTime = TimeSpan.Zero,
                ReturnFlights = []
            };
        }
    }
}
