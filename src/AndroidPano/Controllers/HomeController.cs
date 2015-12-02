using Microsoft.AspNet.Mvc;
using AndroidPano.Services;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http.Features;

namespace AndroidPano.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPanoService panoService;

        public HomeController(IPanoService panoService)
        {
            this.panoService = panoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("{tinyId}")]
        public IActionResult Pano(string tinyId)
        {
            panoService.LoadPano(tinyId);

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
