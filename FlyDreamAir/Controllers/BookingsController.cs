using FlyDreamAir.Data;
using FlyDreamAir.Data.Model;
using FlyDreamAir.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Controllers;

[Route("api/[controller]")]
public class BookingsController: ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AddOnService _addOnService;
    private readonly AirportsService _airportsService;
    private readonly FlightsService _flightsService;

    public BookingsController(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AddOnService addOnService,
        AirportsService airportsService,
        FlightsService flightsService)
    {
        _dbContext = new(dbContextOptions);
        _addOnService = addOnService;
        _airportsService = airportsService;
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
        var addOn = await _addOnService.GetAddOnAsync(id, flightId, departureTime);
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
}
