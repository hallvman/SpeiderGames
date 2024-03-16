using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpeiderGames.Models;

namespace SpeiderGames.Controllers
{
    public class AdminPageController : Controller
    {
        private readonly ILogger<AdminPageController> _logger;
        private readonly IGameService _gameService;

        public AdminPageController(ILogger<AdminPageController> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("/Views/AdminPage/AdminPage.cshtml");
        }
        
        [HttpPost]
        public IActionResult Index(string gameCode)
        {
            var game = _gameService.GetGameByGameCode(gameCode);
            bool isValidGamecode = _gameService.ValidateGameCode(gameCode);

            var validModel = new RequestErrorModel()
            {
                GameCode = gameCode
            };
            
            if (!isValidGamecode)
            {
                return View("Error_Request", validModel);
            }

            return View("/Views/AdminPage/Index.cshtml", game);
        }

        [HttpPost]
        public ActionResult SeePostCodes(Game game)
        {
            var postList = _gameService.GetPostsForGame(game.GameName);
            
            var model = new Game
            {
                GameName = game.GameName,
                Posts = postList
            };
            
            return View("AdminCodePage", model);
        }

        [HttpPost]
        public ActionResult ChangeTeamName()
        {

            return View("Error");
        }
        
        
        [HttpPost]
        public ActionResult ChangeTeamPoints(UpdatePointsViewModel model)
        {
            bool isValidGamecode = _gameService.ValidateGameCode(model.GameCode);

            var validModel = new RequestErrorModel()
            {
                GameName = model.GameName,
                GameCode = model.GameCode
            };
            
            if (!isValidGamecode)
            {
                return View("Error_Request", validModel);
            }
            // Perform any necessary logic based on the selected game
            // For example, you might want to fetch Teams and Posts for the selected game
            var teams = _gameService.GetTeamsForGame(model.GameName);
            var posts = _gameService.GetPostsForGame(model.GameName);
            
            // This assumes that you have properties Teams and Posts in your UpdatePointsViewModel
            model.Teams = new SelectList(teams, "TeamName", "TeamName");
            model.Posts = new SelectList(posts, "PostName", "PostName");

            return View("../PostCoordinatorPage/UpdatePoints", model);
        }
    }
}