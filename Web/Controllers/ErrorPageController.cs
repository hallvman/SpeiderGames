using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;

namespace SpeiderGames.Controllers
{
    public class ErrorPageController : Controller
    {
        [HttpGet]
        [Route("Error/{statusCode}")]
        public ActionResult Index(int statusCode)
        {
            var model = new ErrorModel()
            {
                ErrorCode = statusCode,
            };
            
            // Error handling logic here
            return View("Error", model);
        }
    }
}