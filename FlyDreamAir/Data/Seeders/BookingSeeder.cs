using FlyDreamAir.Data.Db;
using FlyDreamAir.Services;
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

        // AddOns
        // Luggage
        await dbContext.UpdateLuggage(20, 50);
        await dbContext.UpdateLuggage(40, 80);

        // Meals
        await dbContext.UpdateMeal("Curry", "Vegan", 10.0m, new Uri("https://www.justonecookbook.com/wp-content/uploads/2020/02/Vegetarian-Curry-4360-I.jpg"));
        await dbContext.UpdateMeal("Banh mi", "Beef", 9.0m, new Uri("https://images.getrecipekit.com/20230813061131-andy-20cooks-20-20roast-20pork-20banh-20mi.jpg?aspect_ratio=4:3&quality=90&"));

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
        // The flight itself.
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

        // Departure times.
        var airportService = new AirportsService();
        var departureAirport = await airportService.GetAirportAsync(fromAirport)
            ?? throw new ArgumentException("Bad airport", nameof(fromAirport));
        var departureTimeZone = TimeZoneInfo.FindSystemTimeZoneById(departureAirport.TimeZone);

        foreach (var departureTime in departureTimes)
        {
            var schedule = TimeZoneInfo.ConvertTimeToUtc(
                TimeZoneInfo.ConvertTime(DateTimeOffset.Now, departureTimeZone).Date
                    + departureTime.ToTimeSpan(),
                departureTimeZone
            );

            var now = DateTime.UtcNow;
            if (schedule < now)
            {
                schedule = schedule.AddDays(1);
            }

            await dbContext.ScheduledFlights.Include(f => f.Flight)
                .Where(f => f.Flight.FlightId == flightId && f.DepartureTime >= now)
                .ExecuteDeleteAsync();

            for (int i = 0; i < 10; ++i)
            {
                var current = schedule.AddDays(i);
                // ExecuteDeleteAsync above should have nuked all relevant entities.
                dbContext.Entry(new ScheduledFlight()
                {
                    Flight = flight,
                    DepartureTime = current
                }).State = EntityState.Added;
            }
        }

        // Seat data.

        // 1xxx: Domestic
        // 2xxx: International
        var isInternational = flightId[3] - '0' > 1;

        // Solution for this:
        // - Sort everything by rows. Ignore missing rows.
        // - Sort every row by letters.
        // - Breakpoints are:
        //   + Between C and D
        //   + Between G and H
        // - Add a flag to domain class to indicate emergency row.

        await dbContext.Seats.Include(s => s.Flight)
            .ExecuteDeleteAsync();

        var layout = new List<(IEnumerable<int> Rows, string Letters, SeatType Class)>();
        var exitRows = new HashSet<int>();

        static IEnumerable<int> RangeFromTo(int from, int to)
        {
            return Enumerable.Range(from, to - from + 1);
        }

        if (!isInternational)
        {
            // Domestic flights use A320.
            // Seats layout is always 3/3 - ABC/DEF.
            // - 4 Business Class rows (1 - 4).
            // - 5 Premium Class rows (6 - 10).
            // - 20 Economy Class rows (11 - 12, 14 - 30).
            // Exit rows are 12 and 14.

            exitRows = new HashSet<int>([12, 14]);
            layout.Add((
                RangeFromTo(1, 4),
                "ABC/DEF",
                SeatType.Business
            ));
            layout.Add((
                RangeFromTo(6, 10),
                "ABC/DEF",
                SeatType.Premium
            ));
            layout.Add((
                RangeFromTo(11, 12).Concat(RangeFromTo(14, 30)),
                "ABC/DEF",
                SeatType.Economy
            ));
        }
        else // if (isInternational)
        {
            // International flights use A330.
            // Seats layout includes 2/2/2, 2/4/2, and 2/3/2
            // - 3 Business Class rows (1 - 3) (2/2/2 - AC/DG/HK).
            // - 6 Premium Class rows (7 - 12) (2/4/2 - AC/DEFG/HK)
            // - 20 + 16 Economy Class rows (14 - 33, 36 - 51)
            // (2/4/2 - AC/DEFG/HK or 2/3/2 - AC/DEG/HK). Second layout is used from row 47.
            // Exit rows are 14 and 36.

            exitRows = new HashSet<int>([14, 36]);
            layout.Add((
                RangeFromTo(1, 3),
                "AC/DG/HK",
                SeatType.Business
            ));
            layout.Add((
                RangeFromTo(7, 12),
                "AC/DEFG/HK",
                SeatType.Premium
            ));
            layout.Add((
                RangeFromTo(14, 33).Concat(RangeFromTo(36, 46)),
                "AC/DEFG/HK",
                SeatType.Economy
            ));
            layout.Add((
                RangeFromTo(47, 51),
                "AC/DEG/HK",
                SeatType.Economy
            ));
        }

        foreach (var (rows, letters, @class) in layout)
        {
            foreach (var row in rows)
            {
                foreach (var letter in letters.Where(char.IsAsciiLetter))
                {
                    dbContext.Entry(new Seat()
                    {
                        Name = $"{nameof(Seat)} {row}{letter} - " +
                               $"{Enum.GetName(@class)} Class",
                        Type = nameof(Seat),
                        Price = decimal.Round(
                            @class switch
                            {
                                SeatType.Economy => 0m,
                                SeatType.Premium => flight.BaseCost * 0.1m,
                                SeatType.Business => flight.BaseCost * 1.5m,
                                _ => throw new InvalidOperationException("Invalid seat type")
                            }
                        ),
                        Flight = flight,
                        SeatType = @class,
                        SeatRow = row,
                        SeatPosition = letter,
                        IsEmergencyRow = exitRows.Contains(row)
                    }).State = EntityState.Added;
                }
            }
        }
    }

    private static async Task UpdateLuggage(
        this ApplicationDbContext dbContext,
        decimal amount,
        decimal price
    )
    {
        await dbContext.Luggage.Where(l => l.Amount == amount)
            .ExecuteDeleteAsync();

        dbContext.Entry(new Luggage()
        {
            Name = $"Additional Luggage - {amount}kg",
            Type = nameof(Luggage),
            Price = price,
            ImageSrc = new Uri("https://media.istockphoto.com/id/474510508/photo/purple-suitcase-isolated-on-white-background.jpg?s=612x612&w=0&k=20&c=vUvZv4JfS43vodCyXJb_JlH9mKbPcBtdgr0mmrjjejY="),
            Amount = amount
        }).State = EntityState.Added;
    }

    private static async Task UpdateMeal(
        this ApplicationDbContext dbContext,
        string dishName,
        string description,
        decimal price,
        Uri imageSrc
    )
    {
        await dbContext.Meals.Where(m => m.DishName == dishName)
            .ExecuteDeleteAsync();

        dbContext.Entry(new Meal()
        {
            Name = $"Hot Meal - {dishName}",
            Type = nameof(Meal),
            Price = price,
            ImageSrc = imageSrc,
            DishName = dishName,
            Description = description
        }).State = EntityState.Added;
    }
}
