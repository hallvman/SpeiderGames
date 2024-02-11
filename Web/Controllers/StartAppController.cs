using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using System.Diagnostics;

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
