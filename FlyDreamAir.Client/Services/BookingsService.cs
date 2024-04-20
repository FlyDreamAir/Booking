using FlyDreamAir.Data.Model;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace FlyDreamAir.Client.Services;

public class BookingsService
{
    private const string _apiBase = "api/Bookings";
    private readonly HttpClient _httpClient;

    public BookingsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IAsyncEnumerable<AddOn> GetAddOnsAsync(
        string flightId,
        DateTimeOffset departureTime,
        string? type,
        bool? includeSeats
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
        return _httpClient.GetFromJsonAsync<AddOn>(_GetApiUri(new()
        {
            { nameof(id), id },
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime }
        }))!;
    }

    public IAsyncEnumerable<Airport> GetAirportsAsync()
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Airport>();
    }

    public Task<Airport> GetAirportAsync(
        string id
    )
    {
        return _httpClient.GetFromJsonAsync<Airport>(_GetApiUri(new()
        {
            { nameof(id), id }
        }))!;
    }

    public Task<Flight> GetFlightAsync(
        string flightId,
        DateTimeOffset departureTime,
        bool searchPast = false
    )
    {
        return _httpClient.GetFromJsonAsync<Flight>(_GetApiUri(new()
        {
            { nameof(flightId), flightId },
            { nameof(departureTime), departureTime },
            { nameof(searchPast), searchPast }
        }))!;
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

    private async IAsyncEnumerable<T> _GetObjectsFromJsonAsAsyncEnumerable<T>(
        Dictionary<string, object?>? args = null,
        [CallerMemberName] string caller = "")
    {
        await foreach (var obj in
            _httpClient.GetFromJsonAsAsyncEnumerable<T>(_GetApiUri(args, caller)))
        {
            if (obj is not null)
            {
                yield return obj;
            }
        }
    }

    private string _GetApiUri(
        Dictionary<string, object?>? args = null,
        [CallerMemberName] string caller = "")
    {
        const string async = "Async";
        if (caller.EndsWith(async))
        {
            caller = caller[0..(caller.Length - async.Length)];
        }
        if (args is null)
        {
            return $"{_apiBase}/{caller}";
        }
        else
        {
            return QueryHelpers.AddQueryString($"{_apiBase}/{caller}", args.Select((kvp) =>
            {
                return new KeyValuePair<string, string?>(kvp.Key, kvp.Value switch
                {
                    bool b => b ? "true" : "false",
                    _ => kvp.Value?.ToString()
                });
            }));
        }
    }
}
