using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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