using System.Net.Http.Json;

namespace FlyDreamAir.Client.Services;

public class AdminService : HttpApiService
{
    public AdminService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task SeedAddOnsAsync()
    {
        return _PostVoidAsync(caller: "Db/Seed/AddOns");
    }

    public Task SeedFlightsAsync()
    {
        return _PostVoidAsync(caller: "Db/Seed/Flights");
    }

    public Task NukeAsync()
    {
        return _PostVoidAsync(caller: $"Db/{nameof(NukeAsync)}");
    }

    public async IAsyncEnumerable<string[]> ExecuteAsync(string query)
    {
        var response = await _PostAsync(
            content: new StringContent(query),
            caller: $"Db/{nameof(ExecuteAsync)}"
        );

        if (response.IsSuccessStatusCode)
        {
            await foreach (var row in
                response.Content.ReadFromJsonAsAsyncEnumerable<string[]>())
            {
                if (row is not null)
                {
                    yield return row;
                }
            }
        }
        else
        {
            throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}
