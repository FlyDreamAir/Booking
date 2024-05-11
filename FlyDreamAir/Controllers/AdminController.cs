using FlyDreamAir.Data;
using FlyDreamAir.Data.Seeders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FlyDreamAir.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = "FlyDreamAirEmployee")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AddOnsSeeder _addOnsSeeder;
    private readonly FlightsSeeder _flightsSeeder;

    public AdminController(
        DbContextOptions<ApplicationDbContext> dbContextOptions,
        AddOnsSeeder addOnsSeeder,
        FlightsSeeder flightsSeeder
    )
    {
        _dbContext = new ApplicationDbContext(dbContextOptions);
        _addOnsSeeder = addOnsSeeder;
        _flightsSeeder = flightsSeeder;
    }

    [HttpPost("Db/Seed/AddOns")]
    public async Task<IActionResult> SeedAddOns()
    {
        try
        {
            await _addOnsSeeder.Seed();
            return Ok();
        }
        catch
        {
            return UnprocessableEntity();
        }
    }

    [HttpPost("Db/Seed/Flights")]
    public async Task<IActionResult> SeedFlights()
    {
        try
        {
            await _flightsSeeder.Seed();
            return Ok();
        }
        catch
        {
            return UnprocessableEntity();
        }
    }

    [HttpPost($"Db/{nameof(Nuke)}")]
    public async Task<IActionResult> Nuke()
    {
        try
        {
            await _dbContext.OrderedAddOns.ExecuteDeleteAsync();
            await _dbContext.AddOns.ExecuteDeleteAsync();
            await _dbContext.Tickets.ExecuteDeleteAsync();
            await _dbContext.ScheduledFlights.ExecuteDeleteAsync();
            await _dbContext.Flights.ExecuteDeleteAsync();
            await _dbContext.Payments.ExecuteDeleteAsync();
            await _dbContext.Bookings.ExecuteDeleteAsync();
            await _dbContext.Customers.ExecuteDeleteAsync();

            return Ok();
        }
        catch
        {
            return UnprocessableEntity();
        }
    }

    [HttpPost($"Db/{nameof(Execute)}")]
    public async Task<ActionResult<IAsyncEnumerable<string?[]>>> Execute()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var query = await reader.ReadToEndAsync();

            var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            await _dbContext.Database.OpenConnectionAsync();
            var result = await command.ExecuteReaderAsync();

            async IAsyncEnumerable<string?[]> ExecuteCore()
            {
                yield return [result.RecordsAffected.ToString()];

                yield return Enumerable.Range(0, result.FieldCount)
                    .Select(result.GetName)
                    .ToArray();

                while (await result.ReadAsync())
                {
                    yield return Enumerable.Range(0, result.FieldCount)
                        .Select(i => result[i]?.ToString())
                        .ToArray();
                }

                await result.DisposeAsync();
                await command.DisposeAsync();
            }

            return Ok(ExecuteCore());
        }
        catch (Exception e)
        {
            return BadRequest(e.ToString());
        }
    }
}
