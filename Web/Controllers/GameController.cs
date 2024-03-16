// GameController.cs
using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


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
    
    [HttpGet]
    public IActionResult Index()
    {
        return View("/Views/Pages/CreateGamePage.cshtml");
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
    public ActionResult CreateTeamAndPosts(List<Team> Teams)
    {
        string gameName = TempData["GameName"] as string;
        int numberOfPosts = Convert.ToInt32(TempData["NumberOfPosts"]);
        int numberOfTeams = Convert.ToInt32(TempData["NumberOfTeams"]);
        string postsJson = TempData["Posts"] as string;
        string gameCode = TempData["GameCode"] as string;
        string fullPhoneNumber = TempData["FullPhoneNumber"] as string;
        
        List<Post> posts = JsonConvert.DeserializeObject<List<Post>>(postsJson) ?? new List<Post>();
        
        List<Team> tempTeam = new List<Team>();
        
        foreach (var team in Teams)
        {
            var tempName = team.TeamName;
            tempTeam.Add(new Team { TeamName = tempName, Posts = posts});
        }

        var model = new Game
        {
            GameName = gameName,
            FullPhoneNumber = fullPhoneNumber,
            GameCode = gameCode,
            NumberOfTeams = numberOfTeams,
            NumberOfPosts = numberOfPosts,
            Teams = tempTeam,
            Posts = posts
        };

        try
        {
            SendGameCodeSMS(gameName, fullPhoneNumber, gameCode);
            _dbContext.Games.InsertOne(model);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return RedirectToAction("Index", "SuccessPage", model); // Redirect to the home page or any other page
    }
    
    public void SendGameCodeSMS(string gameName, string toPhoneNumber, string gameCode)
    {
        string accountSid = _configuration["Twilio:AccountSID"];
        string authToken = _configuration["Twilio:Authtoken"];

        string phoneNumber = _configuration["Twilio:PhoneNumber"];
        
        // Initialize Twilio client
        TwilioClient.Init(accountSid, authToken);

        var message = MessageResource.Create(
            body: $"Navn: {gameName} & Kode: {gameCode}",
            from: new PhoneNumber(phoneNumber),
            to: new PhoneNumber(toPhoneNumber)
        );

        Console.WriteLine(message.Sid); // Optionally log the message SID
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