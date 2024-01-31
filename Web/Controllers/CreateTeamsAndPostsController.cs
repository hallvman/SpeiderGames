using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
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
    
    [HttpPost]
    public ActionResult CreateTeamAndPosts(Game game)
    {
        try
        {
            // Populate the Posts list
            for (int i = 1; i <= game.NumberOfPosts; i++)
            {
                game.Posts.Add(new Post { Name = $"Post{i}", Points = 0 });
            }

            // Populate the Teams list
            for (int i = 1; i <= game.NumberOfTeams; i++)
            {
                game.Teams.Add(new Team { Name = $"Team{i}", Points = 0 });
            }

            // Insert the game into the MongoDB collection
            _dbContext.Games.InsertOne(game);

            return RedirectToAction("Index", "SuccessPage"); // Redirect to the home page or any other page
        }
        catch (Exception ex)
        {
            // Handle exceptions, log errors, etc.
            return View("Error");
        }
    }
}