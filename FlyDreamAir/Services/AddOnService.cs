using Model = FlyDreamAir.Data.Model;
using FlyDreamAir.Data;
using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Services;

public class AddOnService
{
    private readonly ApplicationDbContext _dbContext;

    public AddOnService(
        DbContextOptions<ApplicationDbContext> dbContextOptions
    )
    {
        _dbContext = new(dbContextOptions);
    }

    public async IAsyncEnumerable<Model.Seat> GetSeatsAsync(
        string flightId,
        DateTimeOffset departureTime,
        Model.SeatType? seatType
    )
    {
        var query = _dbContext.Seats.Include(s => s.Flight)
            .Where(s => s.Flight.FlightId == flightId);

        if (seatType.HasValue)
        {
            if (!Enum.IsDefined(seatType.Value))
            {
                yield break;
            }

            var dbType = seatType.Value switch
            {
                Model.SeatType.Economy => SeatType.Economy,
                Model.SeatType.Premium => SeatType.Premium,
                Model.SeatType.Business => SeatType.Business,
                _ => throw new InvalidOperationException("Should be unreachable.")
            };

            query = query.Where(s => s.SeatType == dbType);
        }

        query = query.OrderBy(q => q.SeatRow)
            .ThenBy(q => q.SeatPosition);

        foreach (var seat in await query.ToListAsync())
        {
            var available = !await _dbContext.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Flight.Flight)
                .Where(t => t.Flight.Flight.FlightId == flightId
                    && t.Flight.DepartureTime == departureTime
                    && t.Seat == seat)
                .AnyAsync();

            var modelType = seat.SeatType switch
            {
                SeatType.Economy => Model.SeatType.Economy,
                SeatType.Premium => Model.SeatType.Premium,
                SeatType.Business => Model.SeatType.Business,
                _ => throw new InvalidOperationException("Invalid seat type.")
            };

            yield return new()
            {
                Id = seat.Id,
                Name = seat.Name,
                Type = seat.Type,
                Price = seat.Price,
                SeatType = modelType,
                SeatRow = seat.SeatRow,
                SeatPosition = seat.SeatPosition,
                IsAvailable = available,
                IsEmergencyRow = seat.IsEmergencyRow
            };
        }
    }
}
