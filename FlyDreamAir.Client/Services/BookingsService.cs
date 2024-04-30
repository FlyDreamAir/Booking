using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Client.Services;

public class BookingsService : HttpApiService
{
    public BookingsService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public IAsyncEnumerable<AddOn> GetAddOnsAsync(
        string flightId,
        DateTimeOffset departureTime,
        string? type = null,
        bool? includeSeats = null
    )
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<AddOn>(new()
        {
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime },
            { nameof(type), type },
            { nameof(includeSeats), includeSeats },
        });
    }

    public Task<AddOn> GetAddOnAsync(
        Guid id,
        string flightId,
        DateTimeOffset departureTime
    )
    {
        return _GetObjectFromJsonAsync<AddOn>(new()
        {
            { nameof(id), id },
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime }
        });
    }

    public IAsyncEnumerable<Airport> GetAirportsAsync()
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Airport>();
    }

    public Task<Airport> GetAirportAsync(
        string id
    )
    {
        return _GetObjectFromJsonAsync<Airport>(new()
        {
            { nameof(id), id }
        });
    }

    public IAsyncEnumerable<Booking> GetBookingsAsync(
        bool includePast = false,
        bool includeUnpaid = false
    )
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Booking>(new()
        {
            { nameof(includePast), includePast },
            { nameof(includeUnpaid), includeUnpaid },
        });
    }

    public Task<Booking> GetBookingAsync(
        Guid id
    )
    {
        return _GetObjectFromJsonAsync<Booking>(new()
        {
            { nameof(id), id }
        });
    }

    public Task<Flight> GetFlightAsync(
        string flightId,
        DateTimeOffset departureTime,
        bool searchPast = false
    )
    {
        return _GetObjectFromJsonAsync<Flight>(new()
        {
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime },
            { nameof(searchPast), searchPast }
        });
    }

    public IAsyncEnumerable<Journey> GetJourneysAsync(
        string from,
        string to,
        DateTimeOffset date,
        DateTimeOffset? returnDate
    )
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Journey>(new()
        {
            { nameof(from), from },
            { nameof(to), to },
            { nameof(date), date },
            { nameof(returnDate), returnDate }
        });
    }

    public IAsyncEnumerable<Seat> GetSeatsAsync(
        string flightId,
        DateTimeOffset departureTime,
        SeatType? seatType
    )
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Seat>(new()
        {
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime },
            { nameof(seatType), seatType }
        });
    }
}
