using FlyDreamAir.Data.Model;
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

    public async IAsyncEnumerable<Airport> GetAirportsAsync()
    {
        await foreach (var airport in
            _httpClient.GetFromJsonAsAsyncEnumerable<Airport>(_GetApiUri()))
        {
            if (airport is not null)
            {
                yield return airport;
            }
        }
    }

    private string _GetApiUri([CallerMemberName] string caller = "")
    {
        const string async = "Async";
        if (caller.EndsWith(async))
        {
            caller = caller[0..(caller.Length - async.Length)];
        }
        return $"{_apiBase}/{caller}";
    }
}
