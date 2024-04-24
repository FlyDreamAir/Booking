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

    public async IAsyncEnumerable<Model.AddOn> GetAddOnsAsync(
        string flightId,
        DateTimeOffset depatureTime,
        string? type,
        bool? includeSeats
    )
    {
        includeSeats ??= false;
        var includeLuggage = type is null || type == nameof(Luggage);
        var includeMeals = type is null || type == nameof(Meal);

        if (includeLuggage)
        {
            await foreach (var luggage in _dbContext.Luggage.AsAsyncEnumerable())
            {
                yield return new Model.Luggage()
                {
                    Id = luggage.Id,
                    Name = luggage.Name,
                    Type = luggage.Type,
                    Price = luggage.Price,
                    ImageSrc = luggage.ImageSrc,
                    Amount = luggage.Amount,
                };
            }
        }

        if (includeMeals)
        {
            await foreach (var meal in _dbContext.Meals.AsAsyncEnumerable())
            {
                yield return new Model.Meal()
                {
                    Id = meal.Id,
                    Name = meal.Name,
                    Type = meal.Type,
                    Price = meal.Price,
                    ImageSrc = meal.ImageSrc,
                    DishName = meal.DishName,
                    Description = meal.Description,
                };
            }
        }

        if (includeSeats.Value)
        {
            await foreach (var seat in GetSeatsAsync(flightId, depatureTime, null))
            {
                if (seat.IsAvailable)
                {
                    yield return seat;
                }
            }
        }
    }

    public async Task<Model.AddOn?> GetAddOnAsync(
        Guid id,
        string flightId,
        DateTimeOffset departureTime,
        bool includeUnavailableSeats
    )
    {
        var addOn = await _dbContext.AddOns.SingleOrDefaultAsync(a => a.Id == id);

        switch (addOn)
        {
            case null:
                return null;
            case Seat seat:
            {
                var seatWithFlight = await _dbContext.Seats.Include(s => s.Flight)
                    .Where(s => s.Flight.FlightId == flightId && s.Id == id)
                    .SingleOrDefaultAsync();

                if (seatWithFlight is null)
                {
                    return null;
                }

                var available = !await _dbContext.Tickets
                    .Include(t => t.Flight)
                    .Include(t => t.Flight.Flight)
                    .Where(t => t.Flight.Flight.FlightId == flightId
                        && t.Flight.DepartureTime == departureTime
                        && t.Seat == seat)
                    .AnyAsync();

                if (!available && !includeUnavailableSeats)
                {
                    return null;
                }

                var modelType = seat.SeatType switch
                {
                    SeatType.Economy => Model.SeatType.Economy,
                    SeatType.Premium => Model.SeatType.Premium,
                    SeatType.Business => Model.SeatType.Business,
                    _ => throw new InvalidOperationException("Invalid seat type.")
                };

                return new Model.Seat()
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
            case Luggage luggage:
            {
                return new Model.Luggage()
                {
                    Id = luggage.Id,
                    Name = luggage.Name,
                    Type = luggage.Type,
                    Price = luggage.Price,
                    ImageSrc = luggage.ImageSrc,
                    Amount = luggage.Amount,
                };
            }
            case Meal meal:
            {
                return new Model.Meal()
                {
                    Id = meal.Id,
                    Name = meal.Name,
                    Type = meal.Type,
                    Price = meal.Price,
                    ImageSrc = meal.ImageSrc,
                    DishName = meal.DishName,
                    Description = meal.Description,
                };
            }
            default:
            {
                return null;
            }
        }
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
