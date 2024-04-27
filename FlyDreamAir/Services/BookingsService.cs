using Model = FlyDreamAir.Data.Model;
using FlyDreamAir.Data;
using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FlyDreamAir.Services;

public class BookingsService
{
    private const string _unverifiedSuffix = ".unverified.fly.trungnt2910.com";

    private readonly ApplicationDbContext _dbContext;
    private readonly AddOnService _addOnService;
    private readonly CardService _cardService;
    private readonly FlightsService _flightsService;

    public BookingsService(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AddOnService addOnService,
        CardService cardService,
        FlightsService flightsService
    )
    {
        _dbContext = new(dbContextOptions);
        _addOnService = addOnService;
        _cardService = cardService;
        _flightsService = flightsService;
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
                        $"@{Guid.NewGuid()}{_unverifiedSuffix}"
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

    public async Task ConfirmBookingAsync(
        Guid id
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var booking = await _dbContext.Bookings.Include(b => b.Customer)
            .SingleAsync(b => b.Id == id);

        var customer = booking.Customer;
        var unverified = booking.Customer.Email.EndsWith(_unverifiedSuffix);

        if (!unverified)
        {
            return;
        }

        var email = Encoding.UTF8.GetString(Convert.FromBase64String(
            booking.Customer.Email.Split("@")[0]
        ));

        var verifiedCustomer =
            await _dbContext.Customers.SingleOrDefaultAsync(c => c.Email == email);

        if (verifiedCustomer is not null)
        {
            verifiedCustomer.FirstName = customer.FirstName;
            verifiedCustomer.LastName = customer.LastName;
            verifiedCustomer.PassportId = customer.PassportId;
            verifiedCustomer.DateOfBirth = customer.DateOfBirth;
            _dbContext.Customers.Update(verifiedCustomer);
            _dbContext.Customers.Remove(customer);

            booking.Customer = verifiedCustomer;
            _dbContext.Bookings.Update(booking);
        }
        else
        {
            customer.Email = email;
            _dbContext.Customers.Update(customer);
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<Guid> RequestCancelBookingAsync(
        Guid id
    )
    {
        var booking = _dbContext.Bookings.Single(b => b.Id == id);

        booking.CancellationId = Guid.NewGuid();
        _dbContext.Update(booking);
        await _dbContext.SaveChangesAsync();

        return booking.CancellationId.Value;
    }

    public async Task CancelBookingAsync(
        Guid id,
        Guid cancellationId
    )
    {
        var booking = _dbContext.Bookings.Single(
            b => b.Id == id && b.CancellationId == cancellationId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;
        if (await _dbContext.Tickets.Include(t => t.Flight).Include(t => t.Booking)
                .Where(t => t.Booking.Id == booking.Id)
                .Where(t => t.Flight.DepartureTime <= now)
                .AnyAsync())
        {
            throw new InvalidOperationException("Too late to cancel flight.");
        }

        await _dbContext.OrderedAddOns
            .Include(oa => oa.Ticket)
            .Include(oa => oa.Ticket.Booking)
            .Where(oa => oa.Ticket.Booking.Id == booking.Id)
            .ExecuteDeleteAsync();

        await _dbContext.Tickets
            .Include(t => t.Booking)
            .Where(t => t.Booking.Id == booking.Id)
            .ExecuteDeleteAsync();

        var creditCardPayments = _dbContext.Payments
            .Include(p => p.Booking)
            .Where(p => p.Type == nameof(CreditCardPayment)
                && p.Booking.Id == booking.Id)
            .OfType<CreditCardPayment>();

        await foreach (var payment in creditCardPayments.ToAsyncEnumerable())
        {
            await _cardService.RefundAsync(
                payment.CardName, payment.CardNumber, payment.Amount
            );
        }

        await creditCardPayments.ExecuteDeleteAsync();

        await _dbContext.Bookings.Where(b => b.Id == booking.Id)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async IAsyncEnumerable<Model.Booking> GetBookingsAsync(
        string email,
        bool includePast,
        bool includeUnpaid
    )
    {
        var bookings = _dbContext.Bookings.Include(b => b.Customer)
            .Where(b => b.Customer.Email == email);

        var now = DateTime.UtcNow;

        if (!includePast)
        {
            bookings = bookings.Where(b =>
                _dbContext.Tickets.Include(t => t.Flight).Include(t => t.Booking)
                    .Where(t => t.Booking.Id == b.Id)
                    .Select(t => t.Flight.DepartureTime)
                    .Max()
                > now
            );
        }

        if (!includeUnpaid)
        {
            bookings = bookings.Where(b =>
                _dbContext.Payments.Include(p => p.Booking)
                    .Where(p => p.Booking.Id == b.Id)
                    .Sum(p => p.Amount)
                >=  _dbContext.Tickets.Include(t => t.Flight).Include(t => t.Flight.Flight)
                        .Include(t => t.Booking)
                        .Where(t => t.Booking.Id == b.Id)
                        .Sum(t => t.Flight.Flight.BaseCost)
                    + _dbContext.OrderedAddOns.Include(oa => oa.Ticket)
                        .Include(oa => oa.Ticket.Booking)
                        .Include(oa => oa.AddOn)
                        .Where(oa => oa.Ticket.Booking.Id == b.Id)
                        .Sum(oa => oa.AddOn.Price * oa.Amount)
            );
        }

        var ids = bookings.Select(b => b.Id);

        foreach (var id in await ids.ToListAsync())
        {
            yield return await GetBookingAsync(id);
        }
    }

    public async Task<Model.Booking> GetBookingAsync(
        Guid id
    )
    {
        var booking = await _dbContext.Bookings.SingleAsync(b => b.Id == id);

        var flights = await _dbContext.Tickets
            .Include(t => t.Booking)
            .Include(t => t.Flight)
            .Include(t => t.Flight.Flight)
            .Where(t => t.Booking.Id == id)
            .ToAsyncEnumerable()
            .SelectAwait(async t => (await _flightsService
                .GetFlightAsync(t.Flight.Flight.FlightId, t.Flight.DepartureTime, true))!)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

        var addOns = await _dbContext.OrderedAddOns
            .Include(a => a.AddOn)
            .Include(a => a.Ticket)
            .Include(a => a.Ticket.Booking)
            .Include(a => a.Ticket.Flight)
            .Include(a => a.Ticket.Flight.Flight)
            .Where(a => a.Ticket.Booking.Id == id)
            .ToAsyncEnumerable()
            .SelectAwait(async a => new Model.PreOrderedAddOn()
            {
                AddOn = (await _addOnService
                    .GetAddOnAsync(
                        a.AddOn.Id,
                        a.Ticket.Flight.Flight.FlightId,
                        a.Ticket.Flight.DepartureTime,
                        true))!,
                Flight = flights.Single(f =>
                    f.DepartureTime == a.Ticket.Flight.DepartureTime
                    && f.FlightId == a.Ticket.Flight.Flight.FlightId),
                Amount = a.Amount
            })
            .ToListAsync();

        var returnFlights = new List<Model.Flight>();

        var firstReturnFlight = flights.FirstOrDefault(f => f.From.Id == booking.To);
        var isTwoWay = firstReturnFlight is not null;

        if (isTwoWay)
        {
            returnFlights = flights.SkipWhile(f => f !=  firstReturnFlight).ToList();
            flights = flights.TakeWhile(f => f != firstReturnFlight).ToList();
        }

        static TimeSpan GetEstimatedTime(List<Model.Flight> flights)
        {
            if (!flights.Any())
            {
                return default;
            }
            return flights.Last().DepartureTime + flights.Last().EstimatedTime
                - flights.First().DepartureTime;
        }

        return new Model.Booking()
        {
            Id = id,
            Journey = new()
            {
                From = flights.First().From,
                To = isTwoWay ? firstReturnFlight!.From : flights.Last().To,
                IsTwoWay = isTwoWay,
                BaseCost = flights.Sum(f => f.BaseCost),
                EstimatedTime = GetEstimatedTime(flights),
                ReturnEstimatedTime = GetEstimatedTime(returnFlights),
                Flights = flights,
                ReturnFlights = returnFlights
            },
            AddOns = addOns
        };
    }

    public async Task<string?> GetBookingEmailAsync(
        Guid id
    )
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Customer)
            .SingleOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return null;
        }

        if (booking.Customer.Email.EndsWith(_unverifiedSuffix))
        {
            return null;
        }

        return booking.Customer.Email;
    }

    public async Task PayBookingAsync(
        Guid id,
        string cardName,
        string cardNumber,
        string cardExpiration,
        string cardCvv
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var flightBaseCost = await _dbContext.Tickets
            .Include(t => t.Booking)
            .Include(t => t.Flight)
            .Include(t => t.Flight.Flight)
            .Where(t => t.Booking.Id == id)
            .Select(t => t.Flight.Flight.BaseCost)
            .SumAsync();

        var addOnsCost = await _dbContext.OrderedAddOns
            .Include(a => a.AddOn)
            .Include(a => a.Ticket)
            .Include(a => a.Ticket.Booking)
            .Include(a => a.Ticket.Flight)
            .Include(a => a.Ticket.Flight.Flight)
            .Where(a => a.Ticket.Booking.Id == id)
            .Select(a => a.AddOn.Price * a.Amount)
            .SumAsync();

        var paid = await _dbContext.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking.Id == id)
            .Select(p => p.Amount)
            .SumAsync();

        var due = flightBaseCost + addOnsCost - paid;

        if (due <= 0)
        {
            return;
        }

        if (!await _cardService.ValidateAsync(cardName, cardNumber, cardExpiration, cardCvv))
        {
            throw new InvalidOperationException("Invalid card.");
        }

        await _cardService.ChargeAsync(cardName, cardNumber, cardExpiration, cardCvv, due);

        await _dbContext.Payments.AddAsync(new CreditCardPayment()
        {
            Amount = due,
            Booking = await _dbContext.Bookings.SingleAsync(b => b.Id == id),
            Type = nameof(CreditCardPayment),
            CardName = cardName,
            CardNumber = cardNumber
        });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
