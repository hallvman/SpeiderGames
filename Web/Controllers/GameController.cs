// GameController.cs
using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Newtonsoft.Json;

public class GameController : Controller
{
    private readonly MongoDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public GameController(IConfiguration configuration)
    {
        _configuration = configuration;
        string dbName = _configuration["MongoDBSettings:DatabaseName"];
        string connectionString = _configuration["MongoDBSettings:ConnectionString"];
        _dbContext = new MongoDbContext(connectionString, dbName);
    }
    
    [HttpPost]
    public ActionResult CreateGame(Game game)
    {
        try
        {
            TempData["GameName"] = game.GameName;
            TempData["NumberOfTeams"] = game.NumberOfTeams;
            TempData["NumberOfPosts"] = game.NumberOfPosts;
            TempData["FullPhoneNumber"] = game.FullPhoneNumber;
            
            string gameCode = GenerateGameCode();
            
            // Create a list of teams
            List<Team> teams = new List<Team>();
            List<Post> posts = new List<Post>();    
            
            for (int i = 1; i <= game.NumberOfPosts; i++)
            {
                string postPin = GeneratePostPin();
                posts.Add(new Post { PostName = $"Post{i}", PostPin = postPin, PostPoints = 0});
            }

            for (int i = 1; i <= game.NumberOfTeams; i++)
            {
                teams.Add(new Team { TeamName = $"Team{i}", Posts = posts});
            }
            
            // Serialize teams to JSON
            string teamsJson = JsonConvert.SerializeObject(teams);
            string postsJson = JsonConvert.SerializeObject(posts);

            TempData["Teams"] = teamsJson;
            TempData["Posts"] = postsJson;
            TempData["GameCode"] = gameCode;
            // Initialize the Posts and Teams list
            var newGame = new Game
            {
                GameName = game.GameName,
                FullPhoneNumber = game.FullPhoneNumber,
                NumberOfPosts = game.NumberOfPosts,
                NumberOfTeams = game.NumberOfTeams,
                GameCode = gameCode
            };
            
            return View("/Views/Pages/CreateTeamsAndPostsPage.cshtml", newGame);
        }
        catch (Exception ex)
        {
            // Handle exceptions, log errors, etc.
            return View("Error");
        }
    }
    
    public static string GenerateGameCode()
    {
        Random random = new Random();
        string characters = "ABCDEFGHIJKLMNPQRSTUVWXYZ"; // Characters to choose from
        string numbers = "123456789"; // Numbers to choose from

        // Generate 3 random letters
        string randomCharacters = new string(Enumerable.Repeat(characters, 3)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // Generate 3 random numbers
        string randomNumbers = new string(Enumerable.Repeat(numbers, 3)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // Combine and shuffle the letters and numbers to ensure the code is mixed
        string gameCode = new string((randomCharacters + randomNumbers).ToCharArray()
            .OrderBy(s => random.Next()).ToArray());

        return gameCode;
    }
    public static string GeneratePostPin()
    {
        Random random = new Random();
        string numbers = "0123456789";

        // Generate 4 random numbers
        string randomNumbers = new string(Enumerable.Repeat(numbers, 4)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // Combine and shuffle the letters and numbers to ensure the code is mixed
        string postPin = new string((randomNumbers).ToCharArray()
            .OrderBy(s => random.Next()).ToArray());

        return postPin;
    }
}