using FlyDreamAir.Data;
using FlyDreamAir.Data.Model;
using FlyDreamAir.Services;
using FlyDreamAir.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace FlyDreamAir.Controllers;

[Route("api/News")]
public class NewsController: ControllerBase {
	private readonly NewsService _newsService;
	
	public NewsController(NewsService ns) {
		_newsService = ns;
	}
	
	[HttpGet(nameof(GetNews))]
	public ActionResult<IAsyncEnumerable<News>> GetNews() {
		return Ok(_newsService.GetNewsAsync());
	}
}