using FlyDreamAir.Data;
using FlyDreamAir.Data.Model;
using FlyDreamAir.Services;
using FlyDreamAir.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Security.Claims;

namespace FlyDreamAir.Controllers;

[Route("api/[controller]")]
public class BookingsController: ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly AddOnService _addOnService;
    private readonly AirportsService _airportsService;
    private readonly BookingsService _bookingsService;
    private readonly FlightsService _flightsService;

    public BookingsController(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        IEmailService emailService,
        AddOnService addOnService,
        AirportsService airportsService,
        BookingsService bookingsService,
        FlightsService flightsService)
    {
        _dbContext = new(dbContextOptions);
        _emailService = emailService;
        _addOnService = addOnService;
        _airportsService = airportsService;
        _bookingsService = bookingsService;
        _flightsService = flightsService;
    }

    [HttpGet(nameof(GetAddOns))]
    public ActionResult<IAsyncEnumerable<AddOn>> GetAddOns(
        [FromQuery]
        string flightId,
        [FromQuery]
        DateTimeOffset departureTime,
        [FromQuery]
        string? type,
        [FromQuery]
        bool? includeSeats
    )
    {
        return Ok(_addOnService.GetAddOnsAsync(flightId, departureTime, type, includeSeats));
    }

    [HttpGet(nameof(GetAddOn))]
    public async Task<ActionResult<AddOn>> GetAddOn(
        [FromQuery]
        Guid id,
        [FromQuery]
        string flightId,
        [FromQuery]
        DateTimeOffset departureTime
    )
    {
        var addOn = await _addOnService.GetAddOnAsync(id, flightId, departureTime, false);
        if (addOn is not null)
        {
            return Ok(addOn);
        }
        return NotFound();
    }

    [HttpGet(nameof(GetAirports))]
    public ActionResult<IAsyncEnumerable<Airport>> GetAirports()
    {
        return Ok(_airportsService.GetAirportsAsync());
    }

    [HttpGet(nameof(GetAirport))]
    public async Task<ActionResult<Airport>> GetAirport(
        [FromQuery]
        string id
    )
    {
        var airport = await _airportsService.GetAirportAsync(id);
        if (airport is not null)
        {
            return Ok(airport);
        }
        return NotFound();
    }

    [HttpGet(nameof(GetBooking))]
    public async Task<ActionResult<Booking>> GetBooking(
        [FromQuery]
        Guid id
    )
    {
        try
        {
            var booking = await _bookingsService.GetBookingAsync(id);
            if (booking is null)
            {
                return NotFound();
            }
            return Ok(booking);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet(nameof(GetFlight))]
    public async Task<ActionResult<Flight>> GetFlight(
        [FromQuery]
        string flightId,
        [FromQuery]
        DateTimeOffset departureTime,
        [FromQuery]
        bool? searchPast
    )
    {
        var flight = await _flightsService.GetFlightAsync(
            flightId, departureTime, searchPast ?? false
        );
        return (flight is not null) ? Ok(flight) : NotFound();
    }

    [HttpGet(nameof(GetJourneys))]
    public ActionResult<IAsyncEnumerable<Journey>> GetJourneys(
        [FromQuery]
        string from,
        [FromQuery]
        string to,
        [FromQuery]
        DateTimeOffset date,
        [FromQuery]
        DateTimeOffset? returnDate
    )
    {
        return Ok(_flightsService.GetJourneysAsync(from, to, date, returnDate));
    }

    [HttpGet(nameof(GetSeats))]
    public ActionResult<IAsyncEnumerable<Seat>> GetSeats(
        [FromQuery]
        string flightId,
        [FromQuery]
        DateTimeOffset departureTime,
        [FromQuery]
        SeatType? seatType
    )
    {
        return Ok(_addOnService.GetSeatsAsync(flightId, departureTime, seatType));
    }


    [HttpPost(nameof(CreateBooking))]
    public async Task<ActionResult> CreateBooking(
        [FromForm]
        string firstName,
        [FromForm]
        string lastName,
        [FromForm]
        string email,
        [FromForm]
        string passportId,
        [FromForm]
        DateTimeOffset dateOfBirth,
        [FromForm]
        string from,
        [FromForm]
        string to,
        [FromForm]
        DateTimeOffset date,
        [FromForm]
        DateTimeOffset? returnDate,
        [FromForm(Name = "flightId")]
        IList<string> flightIds,
        [FromForm(Name = "flightDeparture")]
        IList<DateTimeOffset> flightDepartures,
        [FromForm(Name = "addOnId")]
        IList<Guid> addOnIds,
        [FromForm(Name = "addOnFlightIndex")]
        IList<int> addOnFlightIndexes,
        [FromForm(Name = "addOnAmount")]
        IList<decimal> addOnAmounts
    )
    {
        var signedInEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        var verified = signedInEmail == email;
        Guid bookingId;

        try
        {
            // Hack for ASP.NET's lack of DateTimeOffset support
            dateOfBirth = DateTimeOffset
                .Parse(Request.Form[nameof(dateOfBirth)]!, CultureInfo.InvariantCulture);
            date = DateTimeOffset
                .Parse(Request.Form[nameof(date)]!, CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(Request.Form[nameof(returnDate)]))
            {
                returnDate = DateTimeOffset
                    .Parse(Request.Form[nameof(returnDate)]!, CultureInfo.InvariantCulture);
            }

            flightDepartures = Request.Form["flightDeparture"]
                .Select(kvp => DateTimeOffset.Parse(kvp!, CultureInfo.InvariantCulture))
                .ToList();

            bookingId = await _bookingsService.CreateBookingAsync(
                firstName,
                lastName,
                email,
                passportId,
                DateOnly.FromDateTime(dateOfBirth.DateTime),
                from,
                to,
                date,
                returnDate,
                flightIds,
                flightDepartures,
                addOnIds,
                addOnFlightIndexes,
                addOnAmounts,
                verified
            );
        }
        catch
        {
            return this.RedirectWithQuery("/Flights/Information",
                Request.Form.SelectMany(kvp =>
                    kvp.Value.Select(str =>
                        new KeyValuePair<string, object?>(kvp.Key, str)))
                .Concat(new Dictionary<string, object?> { {
                        "error",
                        "Failed to create your booking. Please check your details and try again."
                    } })
                );
        }

        if (verified)
        {
            return this.RedirectWithQuery("/Flights/Payment", new Dictionary<string, object?>()
            {
                { "bookingId", bookingId }
            });
        }
        else
        {
            var link = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/" +
                $"Flights/Confirm?bookingId={bookingId}";
            await _emailService.SendEmailAsync(
                to,
                "Confirm your booking",
                $"Please confirm your booking by clicking here: {link}",
                $@"Please confirm your booking by <a href=""{link}"">clicking here</a>."
            );
            return Redirect("/Flights/CheckEmail");
        }
    }

    [HttpPost(nameof(ConfirmBooking))]
    public async Task<ActionResult> ConfirmBooking(
        [FromForm]
        Guid id
    )
    {
        try
        {
            await _bookingsService.ConfirmBookingAsync(id);

            return this.RedirectWithQuery("/Flights/Payment", new Dictionary<string, object?>()
            {
                { "bookingId", id }
            });
        }
        catch
        {
            return BadRequest();
        }
    }
}
