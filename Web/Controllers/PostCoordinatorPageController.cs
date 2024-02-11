using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SpeiderGames.Controllers
{
    public class PostCoordinatorPageController : Controller
    {
        private readonly ILogger<PostCoordinatorPageController> _logger;
        private readonly IGameService _gameService;
        private readonly MongoDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PostCoordinatorPageController(ILogger<PostCoordinatorPageController> logger, 
            IGameService gameService,
            IConfiguration configuration)
        {
            _logger = logger;
            _gameService = gameService;
            _configuration = configuration;
            string dbName = _configuration["MongoDBSettings:DatabaseName"];
            string connectionString = _configuration["MongoDBSettings:ConnectionString"];
            _dbContext = new MongoDbContext(connectionString, dbName);
        }
        
        [HttpGet]
        public ActionResult Index()
        {
            var games = _gameService.GetGames();
            
            var gamesSelectList = new SelectList(games, "GameName", "GameName");

            var viewModel = new UpdatePointsViewModel
            {
                // Populate Games property with the SelectList
                Games = gamesSelectList,
                // Other properties...
            };
            
            return View("PostCoordinatorPage", viewModel);
        }

        [HttpPost]
        public ActionResult SelectGame(UpdatePointsViewModel model)
        {
            bool isValidGamecode = _gameService.ValidateGameCode(model.GameName, model.GameCode);

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

            // Redirect to the next step (UpdatePoints) with the selected game as a parameter
            return View("UpdatePoints", model);
        }

        [HttpPost]
        public ActionResult UpdatePoints(UpdatePointsViewModel model)
        {
            var updated = _gameService.UpdatePoints(model.GameName, model.TeamName, model.PostName, model.Points);

            if (updated)
            {
                return RedirectToAction("UpdatePoints", "SuccessPage", model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable to update points. Game, team, or post not found.");
                return View(model);
            }
        }
    }
}