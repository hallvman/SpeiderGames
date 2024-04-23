using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            var selectedGameName = Request.Cookies["SelectedGame"];
            if (!string.IsNullOrEmpty(selectedGameName))
            {
                var isValidGame = _gameService.GetGames().Any(g => g.GameName == selectedGameName);
                if (isValidGame)
                {
                    var teams = _gameService.GetTeamsByGameName(selectedGameName);
                    var posts = _gameService.GetPostsByGameName(selectedGameName)
                        .Select(p => new { p.PostName, Description = p.Description ?? ""}).ToList();

                    var game = new UpdatePointsViewModel
                    {
                        GameName = selectedGameName,
                        Teams = new SelectList(teams, "TeamName", "TeamName"),
                        PostDescriptions = posts.ToDictionary(p => p.PostName, p => p.Description)
                    };
                    ViewData["LogoutType"] = "SelectedGame";
                    return View("UpdatePoints", game);
                }
            }
            var games = _gameService.GetGames();
            
            var gamesSelectList = new SelectList(games, "GameName", "GameName");

            var viewModel = new UpdatePointsViewModel
            {
                Games = gamesSelectList,
            };
            
            return View("PostCoordinatorPage", viewModel);
        }

        [HttpPost]
        public ActionResult SelectGame(UpdatePointsViewModel model)
        {
            var teams = _gameService.GetTeamsByGameName(model.GameName);
            var posts = _gameService.GetPostsByGameName(model.GameName)
                .Select(p => new { p.PostName, Description = p.Description ?? ""}).ToList();

            model.Teams = new SelectList(teams, "TeamName", "TeamName");
            model.PostDescriptions = posts.ToDictionary(p => p.PostName, p => p.Description); //

            Response.Cookies.Append("SelectedGame", model.GameName, new CookieOptions
            {
                HttpOnly = true, // More secure by preventing client-side scripts from accessing the cookie
                Secure = true,   // Makes sure the cookie is sent over HTTPS
                MaxAge = TimeSpan.FromHours(6) // Set the cookie to expire in an hour
            });
            ViewData["LogoutType"] = "SelectedGame";
            return View("UpdatePoints", model);
        }

        [HttpPost]
        public ActionResult UpdatePoints(UpdatePointsViewModel model)
        {
            var updated = _gameService.UpdatePoints(model.GameName, model.TeamName, model.PostName, model.PostPin, model.Points);
            
            if (updated)
            {
                var logsUpdated = _gameService.UpdatePointsInLogs(model.GameName, model.TeamName, model.PostName, model.Points, false);
                var updateComplete = new UpdatePointsViewModel
                {
                    GameName = model.GameName,
                    TeamName = model.TeamName,
                    PostName = model.PostName,
                    Points = model.Points,
                    LogUpdate = logsUpdated,
                };
                return RedirectToAction("UpdatePoints", "SuccessPage", updateComplete);
            }
            else
            {
                return RedirectToAction("Index", "ErrorPage");
            }
        }
    }
}