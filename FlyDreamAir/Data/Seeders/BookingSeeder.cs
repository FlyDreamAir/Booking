using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Data.Seeders;

public static class BookingSeeder
{
    public static async Task SeedBookingData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var options = scope.ServiceProvider.GetService<DbContextOptions<ApplicationDbContext>>();

        if (options is null)
        {
            throw new InvalidOperationException("Cannot get DbContext for seeding.");
        }

        var dbContext = new ApplicationDbContext(options);

        // Flights
        // Vietnam
        await dbContext.UpdateFlight(
            "NT 2910", "SYD", "HAN", new TimeSpan(9, 45, 0), 400,
            new TimeOnly(14, 15));
        await dbContext.UpdateFlight(
            "NT 2911", "HAN", "SYD", new TimeSpan(9, 10, 0), 430,
            new TimeOnly(23, 40));

        await dbContext.UpdateFlight(
            "NT 2912", "SYD", "SGN", new TimeSpan(8, 10, 0), 320,
            new TimeOnly(10, 15));
        await dbContext.UpdateFlight(
            "NT 2913", "SGN", "SYD", new TimeSpan(8, 10, 0), 330,
            new TimeOnly(20, 45));

        await dbContext.UpdateFlight(
            "NT 2914", "MEL", "SGN", new TimeSpan(8, 15, 0), 230,
            new TimeOnly(10, 45));
        await dbContext.UpdateFlight(
            "NT 2915", "SGN", "MEL", new TimeSpan(8, 10, 0), 250,
            new TimeOnly(21, 15));

        // Japan
        await dbContext.UpdateFlight(
            "NT 2920", "SYD", "HND", new TimeSpan(9, 45, 0), 710,
            new TimeOnly(11, 50));
        await dbContext.UpdateFlight(
            "NT 2921", "HND", "SYD", new TimeSpan(9, 40, 0), 790,
            new TimeOnly(22, 00));

        // Domestic
        await dbContext.UpdateFlight(
            "NT 1024", "SYD", "MEL", new TimeSpan(1, 35, 0), 120,
            new TimeOnly(9, 00), new TimeOnly(15, 00));
        await dbContext.UpdateFlight(
            "NT 1025", "MEL", "SYD", new TimeSpan(1, 25, 0), 120,
            new TimeOnly(9, 00), new TimeOnly(15, 00));
        await dbContext.UpdateFlight(
            "NT 1026", "SYD", "MEL", new TimeSpan(1, 35, 0), 90,
            new TimeOnly(12, 00));
        await dbContext.UpdateFlight(
            "NT 1027", "MEL", "SYD", new TimeSpan(1, 25, 0), 90,
            new TimeOnly(12, 00));

        await dbContext.SaveChangesAsync();
    }

    private static async Task UpdateFlight(
        this ApplicationDbContext dbContext,
        string flightId,
        string fromAirport,
        string toAirport,
        TimeSpan estimatedTime,
        decimal baseCost,
        params TimeOnly[] departureTimes
    )
    {
        var flight = await dbContext.Flights.FirstOrDefaultAsync(f => f.FlightId == flightId);

        if (flight is null)
        {
            flight = new()
            {
                FlightId = flightId,
                FromAirport = fromAirport,
                ToAirport = toAirport,
                EstimatedTime = estimatedTime,
                BaseCost = baseCost
            };
        }
        else
        {
            flight.FromAirport = fromAirport;
            flight.ToAirport = toAirport;
            flight.EstimatedTime = estimatedTime;
            flight.BaseCost = baseCost;
        }

        dbContext.Update(flight);

        foreach (var departureTime in departureTimes)
        {
            var schedule = (DateTime.Now.Date + departureTime.ToTimeSpan()).ToUniversalTime();

            for (int i = 0; i < 10; ++i)
            {
                var current = schedule.AddDays(i);
                var scheduledFlight = await dbContext.ScheduledFlights
                    .FirstOrDefaultAsync(f => f.DepartureTime == current);
                if (scheduledFlight is null)
                {
                    dbContext.Add(new ScheduledFlight()
                    {
                        Flight = flight,
                        DepartureTime = current
                    });
                }
            }
        }
    }
}
