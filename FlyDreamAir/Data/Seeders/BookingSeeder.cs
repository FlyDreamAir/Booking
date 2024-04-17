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
            "NT 2910", "SYD", "HAN", new TimeSpan(9, 45, 0), 400);
        await dbContext.UpdateFlight(
            "NT 2911", "HAN", "SYD", new TimeSpan(9, 10, 0), 430);

        await dbContext.UpdateFlight(
            "NT 2912", "SYD", "SGN", new TimeSpan(8, 10, 0), 320);
        await dbContext.UpdateFlight(
            "NT 2913", "SGN", "SYD", new TimeSpan(8, 10, 0), 330);

        await dbContext.UpdateFlight(
            "NT 2914", "MEL", "SGN", new TimeSpan(8, 15, 0), 230);
        await dbContext.UpdateFlight(
            "NT 2915", "SGN", "MEL", new TimeSpan(8, 10, 0), 250);

        // Japan
        await dbContext.UpdateFlight(
            "NT 2920", "SYD", "HND", new TimeSpan(9, 45, 0), 710);
        await dbContext.UpdateFlight(
            "NT 2921", "HND", "SYD", new TimeSpan(9, 40, 0), 790);

        // Domestic
        await dbContext.UpdateFlight(
            "NT 1024", "SYD", "MEL", new TimeSpan(1, 35, 0), 90);
        await dbContext.UpdateFlight(
            "NT 1025", "MEL", "SYD", new TimeSpan(1, 25, 0), 90);
    }

    private static async Task UpdateFlight(
        this ApplicationDbContext dbContext,
        string flightId,
        string fromAirport,
        string toAirport,
        TimeSpan estimatedTime,
        decimal baseCost
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
    }
}
