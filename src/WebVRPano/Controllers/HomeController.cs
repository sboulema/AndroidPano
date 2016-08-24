using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebVRPano.Services;

namespace WebVRPano.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPanoService _panoService;

        public HomeController(IPanoService panoService)
        {
            _panoService = panoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("{tinyId}")]
        public IActionResult Pano(string tinyId)
        {
            _panoService.LoadPano(tinyId);

            ViewData["Pano"] = $"androidpano/{tinyId}/tour.xml";

            return View();
        }

        public IActionResult Error()
        {
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>();

            ViewData["Error"] = error.Error.Message;

            return View();
        }
    }
}
