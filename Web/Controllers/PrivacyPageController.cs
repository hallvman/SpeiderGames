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
        
        [HttpGet]
        public IActionResult AcceptPrivacy()
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // Expires in 7 days
                HttpOnly = true, // Enhances security
                Secure = true // Ensures cookie is sent over HTTPS
            };

            Response.Cookies.Append("PrivacyAccepted", "true", cookieOptions);
            ViewData["PrivacyMode"] = "PrivacyAccepted";
            
            return RedirectToAction("Index", "StartApp");
        }
        
        [HttpGet]
        public IActionResult DeclinePrivacy()
        {
            Response.Cookies.Delete("PrivacyAccepted");
            Response.Cookies.Delete("AdminAccessGranted");
            Response.Cookies.Delete("GameCode");
            Response.Cookies.Delete("SelectedGame");

            // Redirect to a confirmation page, login page, or the home page after logging out
            return RedirectToAction("Index", "StartApp");
        }
    }
}