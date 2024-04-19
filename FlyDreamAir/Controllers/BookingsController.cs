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
    private readonly AirportsService _airportsService;
    private readonly FlightsService _flightsService;

    public BookingsController(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AirportsService airportsService,
        FlightsService flightsService)
    {
        _dbContext = new(dbContextOptions);
        _airportsService = airportsService;
        _flightsService = flightsService;
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
}
