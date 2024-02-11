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