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

    public BookingsController(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AirportsService airportsService)
    {
        _dbContext = new(dbContextOptions);
        _airportsService = airportsService;
    }

    [HttpGet("airports")]
    public async Task<ActionResult<IList<Airport>>> GetAirports()
    {
        return await _airportsService.GetAirportsAsync().ToListAsync();
    }
}
