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
