using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;

namespace SpeiderGames.Controllers
{
    public class ErrorPageController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            // Error handling logic here
            return View("Error");
        }
    }
}