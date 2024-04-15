using Microsoft.AspNetCore.Mvc;

namespace SpeiderGames.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult LogoutPostCoordinator()
        {
            // Delete the SelectedGame cookie by setting its expiration to a past date
            Response.Cookies.Delete("SelectedGame");

            // Redirect to a confirmation page, login page, or the home page after logging out
            return RedirectToAction("Index", "StartApp");
        }
        
        public IActionResult LogoutAdmin()
        {
            // Delete the SelectedGame cookie by setting its expiration to a past date
            Response.Cookies.Delete("AdminAccessGranted");
            Response.Cookies.Delete("GameCode");

            // Redirect to a confirmation page, login page, or the home page after logging out
            return RedirectToAction("Index", "StartApp");
        }
    }
}