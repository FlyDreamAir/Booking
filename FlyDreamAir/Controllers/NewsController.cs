using FlyDreamAir.Data.Model;
using FlyDreamAir.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyDreamAir.Controllers;

[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly NewsService _newsService;

    public NewsController(NewsService ns)
    {
        _newsService = ns;
    }

    [HttpGet(nameof(GetPosts))]
    public ActionResult<IAsyncEnumerable<Post>> GetPosts()
    {
        return Ok(_newsService.GetPosts());
    }
}
