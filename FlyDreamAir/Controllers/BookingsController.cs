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
    public async Task<ActionResult<IList<Airport>>> GetAirports()
    {
        return await _airportsService.GetAirportsAsync().ToListAsync();
    }

    [HttpGet(nameof(GetJourneys))]
    public async Task<ActionResult<IList<Journey>>> GetJourneys(
        [FromQuery]
        string from,
        [FromQuery]
        string to,
        [FromQuery]
        DateTime date
    )
    {
        return await _flightsService.GetJourneysAsync(from, to, date).ToListAsync();
    }
}
