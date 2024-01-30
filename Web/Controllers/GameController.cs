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
    public ActionResult CreateGame(GameViewModel game)
    {
        try
        {
            // You can add validation logic here
            
            // Insert the game into the MongoDB collection
            _dbContext.Games.InsertOne(game);

            return RedirectToAction("Index", "CreateGamePage"); // Redirect to the home page or any other page
        }
        catch (Exception ex)
        {
            // Handle exceptions, log errors, etc.
            return View("Error");
        }
    }
}