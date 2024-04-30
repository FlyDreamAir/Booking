using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Client.Services;

public class NewsService : HttpApiService
{
    public NewsService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public IAsyncEnumerable<Post> GetPostsAsync()
    {
        return _GetObjectsFromJsonAsAsyncEnumerable<Post>();
    }
}
