using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace FlyDreamAir.Client.Services;

public abstract class HttpApiService
{
    private readonly string _apiBase;
    private readonly HttpClient _httpClient;

    public HttpApiService(HttpClient httpClient)
    {
        _apiBase = $"api/{_TrimSuffix(GetType().Name, "Service")}";
        _httpClient = httpClient;
    }

    protected Task<T> _GetObjectFromJsonAsync<T>(
        Dictionary<string, object?>? args = null,
        [CallerMemberName] string caller = ""
    )
    {
        return _httpClient.GetFromJsonAsync<T>(_GetApiUri(args, caller))!;
    }

    protected async IAsyncEnumerable<T> _GetObjectsFromJsonAsAsyncEnumerable<T>(
        Dictionary<string, object?>? args = null,
        [CallerMemberName] string caller = ""
    )
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

    protected Task<HttpResponseMessage> _PostAsync(
        Dictionary<string, object?>? args = null,
        HttpContent? content = null,
        [CallerMemberName] string caller = ""
    )
    {
        return _httpClient.PostAsync(_GetApiUri(args, caller), content);
    }

    protected async Task _PostVoidAsync(
        Dictionary<string, object?>? args = null,
        HttpContent? content = null,
        [CallerMemberName] string caller = ""
    )
    {
        var response = await _PostAsync(args, content, caller);
        response.EnsureSuccessStatusCode();
    }

    private string _GetApiUri(
        Dictionary<string, object?>? args = null,
        [CallerMemberName] string caller = ""
    )
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

    private static string _TrimSuffix(string name, string suffix)
    {
        if (name.EndsWith(suffix))
        {
            name = name[0..(name.Length - suffix.Length)];
        }
        return name;
    }
}
