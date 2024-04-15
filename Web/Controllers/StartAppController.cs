using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SpeiderGames.Controllers
{
    public class StartAppController : Controller
    {
        private readonly ILogger<StartAppController> _logger;

        public StartAppController(ILogger<StartAppController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/Pages/StartPage.cshtml");
        }
    }
}
