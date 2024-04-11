using Microsoft.AspNetCore.Mvc;

namespace SpeiderGames.Controllers
{
    public class TermsPageController : Controller
    {
        private readonly ILogger<TermsPageController> _logger;

        public TermsPageController(ILogger<TermsPageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/Pages/TermsPage.cshtml");
        }
    }
}