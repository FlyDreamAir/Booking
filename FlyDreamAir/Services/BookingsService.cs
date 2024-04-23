﻿using Model = FlyDreamAir.Data.Model;
using FlyDreamAir.Data;
using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FlyDreamAir.Services;

public class BookingsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AddOnService _addOnService;
    private readonly FlightsService _flightsService;

    public BookingsService(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AddOnService addOnService,
        FlightsService flightsService
    )
    {
        _dbContext = new(dbContextOptions);
        _addOnService = addOnService;
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

    public async Task ConfirmBookingAsync(
        Guid id
    )
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var booking = await _dbContext.Bookings.Include(b => b.Customer)
            .SingleAsync(b => b.Id == id);

        var customer = booking.Customer;
        var unverified = booking.Customer.Email.EndsWith(".unverified.fly.trungnt2910.com");

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
}
