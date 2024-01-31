using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using System.Diagnostics;

namespace SpeiderGames.Controllers
{
    public class SuccessPageController : Controller
    {
        private readonly ILogger<TakePartPageController> _logger;

        public SuccessPageController(ILogger<TakePartPageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/Pages/SuccessPage.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}