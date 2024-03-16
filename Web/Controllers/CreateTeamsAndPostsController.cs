using Microsoft.AspNetCore.Mvc;
using SpeiderGames.Models;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class CreateTeamsAndPostsController : Controller
{
    private readonly MongoDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public CreateTeamsAndPostsController(IConfiguration configuration)
    {
        _configuration = configuration;
        string dbName = _configuration["MongoDBSettings:DatabaseName"];
        string connectionString = _configuration["MongoDBSettings:ConnectionString"];
        _dbContext = new MongoDbContext(connectionString, dbName);
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
}