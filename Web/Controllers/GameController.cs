// GameController.cs
using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Microsoft.Extensions.Configuration;

public class GameController : Controller
{
    private readonly MongoDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public GameController(IConfiguration configuration)
    {
        _configuration = configuration;
        string dbName = _configuration["DbName"];
        _dbContext = new MongoDbContext(_configuration.GetConnectionString("MongoDB"),  dbName);
    }
    
    [HttpPost]
    public ActionResult CreateGame(Game game)
    {
        try
        {
            // Initialize the Posts and Teams lists
            var newGame = new Game
            {
                GameName = game.GameName,
                NumberOfPosts = game.NumberOfPosts,
                NumberOfTeams = game.NumberOfTeams,
            };
            
            return View("/Views/Pages/CreateTeamsAndPostsPage.cshtml", newGame);
        }
        catch (Exception ex)
        {
            // Handle exceptions, log errors, etc.
            return View("Error");
        }
    }
}