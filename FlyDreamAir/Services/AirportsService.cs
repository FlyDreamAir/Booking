using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Services;

// This data is available online in a few GitHub repositories as .csv files.
// It is tempting to import those files into our internal database and query everything from there.
// However, this data can change from time to time, and we do not want to carry the burden of
// maintaining it in FlyDreamAir.
//
// The class currently contains some dummy data for the prototype. In the future, a real flight
// information API is required.

public class AirportsService
{
    private readonly List<Airport> _airports = [
        new()
        {
            Id = "SYD",
            City = "Sydney",
            Country = "Australia",
            Name = "Sydney Kingsford Smith Airport"
        },
        new()
        {
            Id = "MEL",
            City = "Melbourne",
            Country = "Australia",
            Name = "Melbourne Airport"
        },
        new()
        {
            Id = "HAN",
            City = "Hanoi",
            Country = "Vietnam",
            Name = "Noi Bai International Airport"
        },
        new()
        {
            Id = "SGN",
            City = "Ho Chi Minh City",
            Country = "Vietnam",
            Name = "Tan Son Nhat International Airport"
        },
        new()
        {
            Id = "HND",
            City = "Tokyo",
            Country = "Japan",
            Name = "Haneda Airport"
        }
    ];

    public AirportsService()
    {

    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<Airport> GetAirportsAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var airport in _airports)
        {
            yield return airport;
        }
    }

    public async Task<Airport?> GetAirportAsync(string id)
    {
        return await Task.Run(() =>
        {
            return _airports.FirstOrDefault(a => a.Id == id);
        });
    }
}
