using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

public class CreateTeamsAndPostsController : Controller
{
    private readonly MongoDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public CreateTeamsAndPostsController(IConfiguration configuration)
    {
        _configuration = configuration;
        string dbName = _configuration["DbName"];
        _dbContext = new MongoDbContext(_configuration.GetConnectionString("MongoDB"),  dbName);
    }
    [HttpGet]
    public ActionResult CreateTeamAndPostsPage()
    {
        string gameName = TempData["GameName"] as string;
        int numberOfPosts = Convert.ToInt32(TempData["NumberOfPosts"]);
        int numberOfTeams = Convert.ToInt32(TempData["NumberOfTeams"]);

        // Retrieve teams and posts from TempData
        List<Team> teams = TempData["Teams"] as List<Team> ?? new List<Team>();
        List<Post> posts = TempData["Posts"] as List<Post> ?? new List<Post>();

        // Use the values as needed
        var model = new Game
        {
            GameName = gameName,
            NumberOfTeams = numberOfTeams,
            NumberOfPosts = numberOfPosts,
            Teams = teams,
            Posts = posts
        };
        
        // For example, you can pass the teams and posts to the view
        ViewBag.Teams = teams;
        ViewBag.Posts = posts;

        return View("/Views/Pages/CreateTeamsAndPostsPage.cshtml", model);
    }

    [HttpPost]
    public ActionResult CreateTeamAndPosts()
    {
        // Your logic for creating teams and posts.
        // Insert the game into the MongoDB collection
        
        string gameName = TempData["GameName"] as string;
        int numberOfPosts = Convert.ToInt32(TempData["NumberOfPosts"]);
        int numberOfTeams = Convert.ToInt32(TempData["NumberOfTeams"]);
        string teamsJson = TempData["Teams"] as string;
        string postsJson = TempData["Posts"] as string;
        
        // Deserialize JSON to List<Team>
        List<Team> teams = JsonConvert.DeserializeObject<List<Team>>(teamsJson) ?? new List<Team>();
        List<Post> posts = JsonConvert.DeserializeObject<List<Post>>(postsJson) ?? new List<Post>();

        // Use the values as needed
        var model = new Game
        {
            GameName = gameName,
            NumberOfTeams = numberOfTeams,
            NumberOfPosts = numberOfPosts,
            Teams = teams,
            Posts = posts
        };
        
        _dbContext.Games.InsertOne(model);

        return RedirectToAction("Index", "SuccessPage"); // Redirect to the home page or any other page
    }
}