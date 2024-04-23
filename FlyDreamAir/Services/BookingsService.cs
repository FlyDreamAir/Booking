using FlyDreamAir.Data;
using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FlyDreamAir.Services;

public class BookingsService
{
    private readonly ApplicationDbContext _dbContext;

    public BookingsService(
        DbContextOptions<ApplicationDbContext> dbContextOptions
    )
    {
        _dbContext = new(dbContextOptions);
    }

    public async Task<Guid> CreateBookingAsync(
        string firstName,
        string lastName,
        string email,
        string passportId,
        DateOnly dateOfBirth,
        string from,
        string to,
        DateTimeOffset date,
        DateTimeOffset? returnDate,
        IList<string> flightIds,
        IList<DateTimeOffset> flightDepartures,
        IList<Guid> addOnIds,
        IList<int> addOnFlightIndexes,
        IList<decimal> addOnAmounts,
        bool verified
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        date = date.ToUniversalTime();
        returnDate = returnDate?.ToUniversalTime();
        flightDepartures = flightDepartures.Select(f => f.ToUniversalTime()).ToList();

        Customer? customer = null;
        if (verified)
        {
            customer = _dbContext.Customers.FirstOrDefault(c => c.Email == email);
            if (customer is not null)
            {
                customer.FirstName = firstName;
                customer.LastName = lastName;
                customer.Email = email;
                customer.PassportId = passportId;
                customer.DateOfBirth = dateOfBirth;
                _dbContext.Entry(customer).State = EntityState.Modified;
            }
        }

        if (customer is null)
        {
            customer = new Customer()
            {
                FirstName = firstName,
                LastName = lastName,
                PassportId = passportId,
                DateOfBirth = dateOfBirth,
                Email = verified ? email :
                    $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(email))}" +
                        $"@{Guid.NewGuid()}.unverified.fly.trungnt2910.com"
            };

            await _dbContext.Customers.AddAsync(customer);
        }

        var booking = new Booking()
        {
            Customer = customer,
            From = from,
            To = to
        };

        await _dbContext.Bookings.AddAsync(booking);

        var flights = await flightIds.Zip(flightDepartures).ToAsyncEnumerable()
            .SelectAwait(async p =>
            {
                var (id, departure) = p;
                return await _dbContext.ScheduledFlights.Include(sf => sf.Flight)
                    .SingleAsync(sf => sf.Flight.FlightId == id && sf.DepartureTime == departure);
            }).ToListAsync();

        static bool ValidDate(DateTime date1, DateTimeOffset date2)
        {
            var date3 = date2.ToUniversalTime().DateTime;
            if (date1 <= date3)
            {
                return false;
            }
            return (date1 - date3).TotalHours <= 24;
        }

        if (flights.First().Flight.FromAirport != @from
            || !ValidDate(flights.First().DepartureTime, date))
        {
            throw new InvalidOperationException("Invalid flight list.");
        }

        if (returnDate.HasValue)
        {
            var firstReturnFlight = flights.Single(sf => sf.Flight.FromAirport == to);
            if (!ValidDate(firstReturnFlight.DepartureTime.Date, returnDate.Value))
            {
                throw new InvalidOperationException("Invalid flight list.");
            }
        }

        var addOns = await addOnIds.ToAsyncEnumerable()
            .SelectAwait(async id =>
            {
                return await _dbContext.AddOns.SingleAsync(a => a.Id == id);
            }).ToListAsync();

        var seats = addOns.Select((a, index) => (AddOn: a, Index: index))
            .Where(pair => pair.AddOn.Type == nameof(Seat))
            .OrderBy(pair => addOnFlightIndexes[pair.Index])
            .Select(pair => pair.AddOn);

        foreach (var (flight, seatAddOn) in flights.Zip(seats))
        {
            var seat = await _dbContext.Seats
                .Include(sf => sf.Flight)
                .SingleAsync(s => s.Id == seatAddOn.Id);

            if (seat.Flight.FlightId != flight.Flight.FlightId)
            {
                throw new InvalidOperationException("Invalid seat for flight.");
            }

            var available = !await _dbContext.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Flight.Flight)
                .Where(t => t.Flight.Flight.FlightId == flight.Flight.FlightId
                    && t.Flight.DepartureTime == flight.DepartureTime
                    && t.Seat == seat)
                .AnyAsync();

            if (!available)
            {
                throw new InvalidOperationException("Seat unavailable.");
            }

            var ticket = new Ticket()
            {
                Booking = booking,
                Flight = flight,
                Type = nameof(Ticket),
                HolderFirstName = firstName,
                HolderLastName = lastName,
                Seat = seat
            };

            await _dbContext.Tickets.AddAsync(ticket);

            var currentAddOns = addOns.Zip(addOnFlightIndexes, addOnAmounts)
                .Where(val => flightIds[val.Second] == flight.Flight.FlightId);

            foreach (var (addOn, flightIndex, amount) in currentAddOns)
            {
                var orderedAddOn = new OrderedAddOn()
                {
                    Ticket = ticket,
                    AddOn = addOn,
                    Amount = amount
                };

                await _dbContext.OrderedAddOns.AddAsync(orderedAddOn);
            }
        }

        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return booking.Id;
    }
}
