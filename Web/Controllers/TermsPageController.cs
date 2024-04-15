using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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