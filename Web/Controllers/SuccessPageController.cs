using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpeiderGames.Models;

namespace SpeiderGames.Controllers
{
    public class SuccessPageController : Controller
    {
        private readonly ILogger<SuccessPageController> _logger;

        public SuccessPageController(ILogger<SuccessPageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(Game model)
        {
            return View("/Views/Pages/SuccessPage.cshtml", model);
        }
        
        [HttpGet]
        public IActionResult UpdatePoints(UpdatePointsViewModel model)
        {
            return View("/Views/Pages/SuccessPageUpdatePoints.cshtml", model);
        }
    }
}