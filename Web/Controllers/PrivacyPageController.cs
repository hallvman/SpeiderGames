using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using System.Diagnostics;

namespace SpeiderGames.Controllers
{
    public class PrivacyPageController : Controller
    {
        private readonly ILogger<PrivacyPageController> _logger;

        public PrivacyPageController(ILogger<PrivacyPageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/Pages/PrivacyPage.cshtml");
        }
    }
}