// GameController.cs
using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
            TempData["GameName"] = game.GameName;
            TempData["NumberOfTeams"] = game.NumberOfTeams;
            TempData["NumberOfPosts"] = game.NumberOfPosts;
            // Create a list of teams
            List<Team> teams = new List<Team>();
            List<Post> posts = new List<Post>();

            for (int i = 1; i <= game.NumberOfTeams; i++)
            {
                teams.Add(new Team { Name = $"Team{i}" });
            }
            
            for (int i = 1; i <= game.NumberOfPosts; i++)
            {
                posts.Add(new Post { Name = $"Post{i}" });
            }
            
            // Serialize teams to JSON
            string teamsJson = JsonConvert.SerializeObject(teams);
            string postsJson = JsonConvert.SerializeObject(posts);

            TempData["Teams"] = teamsJson;
            TempData["Posts"] = postsJson;
            // Initialize the Posts and Teams list
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