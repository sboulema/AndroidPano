using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebVRPano.Services;

namespace WebVRPano.Controllers;

[Route("")]
public class HomeController(IPanoService panoService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("{tinyId}")]
    public async Task<IActionResult> Pano(string tinyId)
    {
        await panoService.LoadPano(tinyId);

        ViewData["Pano"] = $"webvrpano/{tinyId}/tour.xml";

        return View();
    }

    [HttpGet("[action]")]
    public IActionResult Error()
    {
        var error = HttpContext.Features.Get<IExceptionHandlerFeature>();

        ViewData["Error"] = error?.Error.Message;

        return View();
    }
}
